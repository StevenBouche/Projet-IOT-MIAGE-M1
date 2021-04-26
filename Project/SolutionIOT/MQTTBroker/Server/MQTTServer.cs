using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MQTTBroker.Server
{

    public class MQTTServer
    {

        static readonly int ConnectionBacklog = 100;
        readonly int Port = 1883;
        readonly string IpAddr = "127.0.0.1";
        static readonly string Username = "clientProjectIOTMIAGE";
        static readonly string Password = "XAjNyUPfS8FmBQ[s";

        readonly IMqttServer server;

        public MQTTServer(string serverAddr, int port)
        {

            this.InitManager();

            this.IpAddr = serverAddr;
            this.Port = port;
            this.server = new MqttFactory().CreateMqttServer();

            this.InitHandler();
            
        }

        private void InitManager()
        {
            

        }

        private void InitHandler()
        {

            this.server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandlerMessageReceived);
            this.server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(HandlerClientConnection);
            this.server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(HandlerClientDisconect);
            this.server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(HandlerClientUnsubscribed);
            this.server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(HandlerClientSubscribed);

        }

        public void StartBroker()
        {

            IPAddress ipAddress = IPAddress.Parse(IpAddr);

            var options = new MqttServerOptionsBuilder()
               .WithConnectionBacklog(ConnectionBacklog)
               .WithDefaultEndpointPort(Port)
                .WithConnectionValidator(c =>
                {
                    Console.WriteLine($"{c.ClientId} connection validator for c.Endpoint: {c.Endpoint}");
                    if ((Username.Equals(c.Username) && Password.Equals(c.Password)))
                        c.ReasonCode = MqttConnectReasonCode.Success;
                    else
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                });

            this.server.StartAsync(options.Build()).Wait();

            Console.WriteLine($"Broker is Running: Host: {server.Options.DefaultEndpointOptions.BoundInterNetworkAddress} Port: {server.Options.DefaultEndpointOptions.Port}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        public void StopBroker()
        {
            this.server.StopAsync().Wait();
        }

        private void HandlerMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {

         
        }

        private async void HandlerClientConnection(MqttServerClientConnectedEventArgs e)
        {

           
        }

        private void HandlerClientDisconect(MqttServerClientDisconnectedEventArgs e)
        {
            

        }

        private void HandlerClientUnsubscribed(MqttServerClientSubscribedTopicEventArgs e)
        {
            
        }

        private void HandlerClientSubscribed(MqttServerClientUnsubscribedTopicEventArgs e)
        {
           
        }

        private async Task<string> GetAddrIpClient(string id)
        {
            IList<IMqttClientStatus> Csl = await server.GetClientStatusAsync();
            IMqttClientStatus Cs = Csl.FirstOrDefault(c => c.ClientId == id);
            return Cs is null ? "" : Cs.Endpoint;
        }
      

    }

}
