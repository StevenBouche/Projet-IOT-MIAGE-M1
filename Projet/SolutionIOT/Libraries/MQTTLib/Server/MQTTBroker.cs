using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using MQTTLib.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MQTTLib.Server
{

    public class MQTTBroker
    {

        static readonly int ConnectionBacklog = 100;
        MQTTConfigBroker Config;
        readonly IMqttServer Server;

        public MQTTBroker(MQTTConfigBroker config)
        {
            this.Config = config;
            this.Server = new MqttFactory().CreateMqttServer();
        }

        public delegate void ClientConnectionHandler(MqttServerClientConnectedEventArgs args);
        public delegate void ClientDisconnectHandler(MqttServerClientDisconnectedEventArgs args);
        public delegate void MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs args);
        public delegate void ClientUnsubscribedHandler(MqttServerClientUnsubscribedTopicEventArgs args);
        public delegate void ClientSubscribedHandler(MqttServerClientSubscribedTopicEventArgs args);
        public delegate void ValidatorConnectionHandler(MqttConnectionValidatorContext args);
        public delegate void ValidatorSubscriptionHandler(MqttSubscriptionInterceptorContext args);
        public delegate void ValidatorMessageHandler(MqttApplicationMessageInterceptorContext args);

        public ClientConnectionHandler ClientConnectedEvents { get; set; }
        public ClientDisconnectHandler ClientDisconnectEvents { get; set; }
        public MessageReceivedHandler MessageReceivedEvents { get; set; }
        public ClientUnsubscribedHandler ClientUnsubscribedEvents { get; set; }
        public ClientSubscribedHandler ClientSubscribedEvents { get; set; }
        public ValidatorConnectionHandler ValidatorConnectionEvents { get; set; }
        public ValidatorSubscriptionHandler ValidatorSubscriptionEvents { get; set; }
        public ValidatorMessageHandler ValidatorMessageEvents { get; set; }

    private void InitHandler()
        {
            Server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e => MessageReceivedEvents?.Invoke(e));
            Server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e => ClientConnectedEvents?.Invoke(e));
            Server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e => ClientDisconnectEvents?.Invoke(e));
            Server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(e => ClientSubscribedEvents?.Invoke(e));
            Server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(e => ClientUnsubscribedEvents?.Invoke(e));
        }

        public Task StartBroker()
        {

            if (Server.IsStarted)
                return Task.CompletedTask;

            this.InitHandler();

            var options = new MqttServerOptionsBuilder()
               .WithConnectionBacklog(ConnectionBacklog)
               .WithDefaultEndpointPort(Config.Port)
               .WithConnectionValidator(c => ValidatorConnectionEvents?.Invoke(c))
               .WithSubscriptionInterceptor(c => ValidatorSubscriptionEvents?.Invoke(c))
               .WithApplicationMessageInterceptor(c => ValidatorMessageEvents?.Invoke(c));

            return Server.StartAsync(options.Build());

            /*Console.WriteLine($"Broker is Running: Host: {server.Options.DefaultEndpointOptions.BoundInterNetworkAddress} Port: {server.Options.DefaultEndpointOptions.Port}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();*/
        }


        public IMqttServer GetMqttServer()
        {
            return this.Server;
        }

        public Task StopBroker()
        {
            if (!Server.IsStarted)
                return Task.CompletedTask;

            return Server.StopAsync();
        }

        public async Task<string> GetAddrIpClient(string id)
        {
            IList<IMqttClientStatus> Csl = await Server.GetClientStatusAsync();
            IMqttClientStatus Cs = Csl.FirstOrDefault(c => c.ClientId == id);
            return Cs is null ? "" : Cs.Endpoint;
        }
    }
}
