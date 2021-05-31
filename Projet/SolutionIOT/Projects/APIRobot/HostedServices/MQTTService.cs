using APIRobot.Configs;
using APIRobot.Configs.HostedServices;
using APIRobot.Models;
using APIRobot.Models.Auth;
using APIRobot.Models.Data;
using APIRobot.Services;
using APIRobot.Services.Cache;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;


namespace APIRobot.HostedServices
{

    public class MQTTService : IHostedService, IDisposable
    {
        //Logger
        private readonly ILogger Logger;

        //Configuration
        private readonly IOptions<MQTTServiceConfig> ConfigService;
        private readonly IOptions<CertificateConfig> ConfigCert;
        //Cache
        private readonly IMQTTConnectionCache ConnectionCache;

        //Actions
        private readonly Dictionary<string, Action<EquipmentValue, MqttApplicationMessageReceivedEventArgs>> TopicActions;

        //Services
        private readonly IMqttServer Server;
        private readonly DataRobotService DataService;
        private readonly IAuthorizationMQTT Authorization;
        private readonly IChannelConnectionCache MotorHandler;

        public MQTTService(
            DataRobotService dataService,
            IAuthorizationMQTT authorization,
            IMQTTConnectionCache cache,
            IChannelConnectionCache motorHandler,
            IOptions<MQTTServiceConfig> configService,
            IOptions<CertificateConfig> configCert,
            ILogger<MQTTService> logger)
        {
            Logger = logger;
            DataService = dataService;
            MotorHandler = motorHandler;
            ConfigService = configService;
            ConfigCert = configCert;
            Authorization = authorization;
            ConnectionCache = cache;
            Server = new MqttFactory().CreateMqttServer();
            TopicActions = new Dictionary<string, Action<EquipmentValue, MqttApplicationMessageReceivedEventArgs>>();
            this.InitTopicActions();
        }

        public void InitTopicActions()
        {
            TopicActions.Add(ConfigService.Value.TopicNameDataRobot, HandlerDataRobot);
            TopicActions.Add(ConfigService.Value.TopicNameMotorDataRobot, HandlerMotorDataRobot);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            if (Server.IsStarted)
                return Task.CompletedTask;

            Server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e => HandlerMessageReceived(e));
            Server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e => HandlerClientConnection(e));
            Server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e => HandlerClientDisconnect(e));
            Server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(e => HandlerClientSubscribed(e));
            Server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(e => HandlerClientUnsubscribed(e));

            var options = new MqttServerOptionsBuilder()
               .WithConnectionBacklog(100)
               .WithDefaultEndpointPort(ConfigService.Value.MQTTConfigBroker.Port)
               .WithConnectionValidator(c => HandlerValidatorConnection(c))
               .WithSubscriptionInterceptor(c => HandlerValidatorSubscription(c))
               .WithApplicationMessageInterceptor(c => HandlerValidatorMessage(c));

            try
            {
                var certificate = new X509Certificate2(ConfigCert.Value.AbsolutePath, ConfigCert.Value.Password, X509KeyStorageFlags.Exportable);

                options.WithEncryptedEndpoint()
                    .WithEncryptedEndpointPort(ConfigService.Value.MQTTConfigBroker.EncryptedPort)
                    .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
                    .WithEncryptionSslProtocol(SslProtocols.Tls12);

            } catch (CryptographicException e)
            {
                Logger.LogError(e.Message);
            }

            Logger.LogInformation("MQTT broker startup.");

            return Server.StartAsync(options.Build());
        }

        private void HandlerValidatorConnection(MqttConnectionValidatorContext args)
        {

            var token = args.Username;
            var canConnect = Authorization.VerifyMQTTAuthorizationConnection(token, out EquipmentIdentity identity);

            if (identity is null)
            {
                Logger.LogInformation($"{args.ClientId} failed connection : Bad authentification token.");
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            }
            else if (!canConnect)
            {
                Logger.LogInformation($"{args.ClientId} failed connection : Not Authorized.");
                args.ReasonCode = MqttConnectReasonCode.NotAuthorized;
            }
            else
            {
                EquipmentValue equipment = ConnectionCache.GetEquipmentMQTT(args.ClientId);

                if (equipment is not null)
                {
                    Logger.LogInformation($"{args.ClientId} failed connection : Is already connected.");
                    args.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    return;
                }

                args.ReasonCode = MqttConnectReasonCode.Success;

                ConnectionCache.ConnectedMQTT(identity, args.ClientId, args.Endpoint);
            }
        }

        private void HandlerValidatorMessage(MqttApplicationMessageInterceptorContext args)
        {
            EquipmentValue equipment = ConnectionCache.GetEquipmentMQTT(args.ClientId);
            args.AcceptPublish = equipment is not null && Authorization.VerifyAuthorizationRoleSenderTopic(equipment, args.ApplicationMessage.Topic);
        }

        private void HandlerValidatorSubscription(MqttSubscriptionInterceptorContext args)
        {
            EquipmentValue equipment = ConnectionCache.GetEquipmentMQTT(args.ClientId);
            args.AcceptSubscription = Authorization.VerifyAuthorizationRoleSubscribeTopic(equipment, args.TopicFilter.Topic);
            args.CloseConnection = !args.AcceptSubscription;
        }

        private void HandlerMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            string topic = e.ApplicationMessage.Topic;
            string clientId = e.ClientId;

            EquipmentValue equipment = ConnectionCache.GetEquipmentMQTT(clientId);

            if (equipment is null)
            {
                return;
            }

            foreach(string keyTopic in TopicActions.Keys)
            {
                if(Authorization.TopicsMatch(keyTopic, topic))
                {
                    TopicActions[keyTopic]?.Invoke(equipment, e);
                    break;
                }
            }

            Logger.LogInformation($"{e.ClientId} published message to topic {e.ApplicationMessage.Topic}.");
        }

        private void HandlerDataRobot(EquipmentValue equipment, MqttApplicationMessageReceivedEventArgs eventMQTT)
        {

            DataRobot data;
            string obj = eventMQTT.ApplicationMessage.ConvertPayloadToString();

            try
            {
                data = JsonConvert.DeserializeObject<DataRobot>(obj);
            }
            catch (Exception e)
            {
                Logger.LogInformation(e.Message);
                return;
            }

            if (!equipment.IdEquipment.Equals(data.IdESP))
                return;

            data.Timestamp = Convert.ToInt64(UnixTimeNow());
            DataService.Create(data);

        }

        private void HandlerMotorDataRobot(EquipmentValue equipment, MqttApplicationMessageReceivedEventArgs eventMQTT)
        {

            MotorValues data;
            string obj = eventMQTT.ApplicationMessage.ConvertPayloadToString();

            string[] termTopicConf = eventMQTT.ApplicationMessage.Topic.Split('/');
            string idEquipment = termTopicConf[2];

            try
            {
                data = JsonConvert.DeserializeObject<MotorValues>(obj);
            }
            catch (Exception e)
            {
                Logger.LogInformation(e.Message);
                return;
            }

            data.Timestamp = Convert.ToInt64(UnixTimeNow());
            Task.Run(() => MotorHandler.GetChannelsEquipment(idEquipment)?.StreamMotor.Writer.WriteAsync(data));
        }

        private void HandlerClientConnection(MqttServerClientConnectedEventArgs args)
        {
            Logger.LogInformation($"{args.ClientId} connected to broker.");
        }

        private void HandlerClientDisconnect(MqttServerClientDisconnectedEventArgs e)
        {
            Logger.LogInformation($"{e.ClientId} disonnected from the broker.");

            ConnectionCache.DisconnectedMQTT(e.ClientId);
        }

        private void HandlerClientUnsubscribed(MqttServerClientUnsubscribedTopicEventArgs e)
        {
            Logger.LogInformation($"{e.ClientId} unsubscribed to {e.TopicFilter}.");
        }

        private void HandlerClientSubscribed(MqttServerClientSubscribedTopicEventArgs e)
        {
            Logger.LogInformation(e.ClientId + " subscribed to " + e.TopicFilter);
        }

        private static double UnixTimeNow()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
            if (!Server.IsStarted)
                return Task.CompletedTask;

            Logger.LogInformation("Stopping MQTT Daemon.");

            return Server.StopAsync();
        }

        public void Dispose()
        {
            
        }
    }
}