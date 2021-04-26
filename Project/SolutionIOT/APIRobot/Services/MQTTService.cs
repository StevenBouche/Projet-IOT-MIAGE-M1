using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTBroker.Config;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APIRobot.Services
{
    public class MQTTService : IHostedService, IDisposable
    {

        private readonly ILogger Logger;
        private readonly IOptions<MQTTConfig> Config;
        private IMqttServer Server;

        public MQTTService(IOptions<MQTTConfig> config, ILogger<MQTTService> logger)
        {
            Logger = logger;
            Config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Server = new MqttFactory().CreateMqttServer();

            var options = new MqttServerOptionsBuilder()
               .WithConnectionBacklog(100)
               .WithDefaultEndpointPort(Config.Value.Port)
               .WithConnectionValidator(c =>
                {
                    /*Console.WriteLine($"{c.ClientId} connection validator for c.Endpoint: {c.Endpoint}");
                    if ((Username.Equals(c.Username) && Password.Equals(c.Password)))
                        c.ReasonCode = MqttConnectReasonCode.Success;
                    else
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;*/
                });

            Server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandlerMessageReceived);
            Server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(HandlerClientConnection);
            Server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(HandlerClientDisconect);
            Server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(HandlerClientUnsubscribed);
            Server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(HandlerClientSubscribed);

            Logger.LogInformation("MQTT broker startup.");

            return Server.StartAsync(options.Build());
        }

        private void HandlerMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            Logger.LogInformation(e.ClientId + " published message to topic " + e.ApplicationMessage.Topic);

        }

        private void HandlerClientConnection(MqttServerClientConnectedEventArgs e)
        {
            Logger.LogInformation(e.ClientId + " Connected.");
        }

        private void HandlerClientDisconect(MqttServerClientDisconnectedEventArgs e)
        {
            Logger.LogInformation(e.ClientId + " Disonnected.");
        }

        private void HandlerClientUnsubscribed(MqttServerClientSubscribedTopicEventArgs e)
        {
            Logger.LogInformation(e.ClientId + " subscribed to " + e.TopicFilter);
        }

        private void HandlerClientSubscribed(MqttServerClientUnsubscribedTopicEventArgs e)
        {
            Logger.LogInformation(e.ClientId + " unsubscribed to " + e.TopicFilter);
        }

        private async Task<string> GetAddrIpClient(string id)
        {
            IList<IMqttClientStatus> Csl = await Server.GetClientStatusAsync();
            IMqttClientStatus Cs = Csl.FirstOrDefault(c => c.ClientId == id);
            return Cs is null ? "" : Cs.Endpoint;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping MQTT Daemon.");
            return Server.StopAsync();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}