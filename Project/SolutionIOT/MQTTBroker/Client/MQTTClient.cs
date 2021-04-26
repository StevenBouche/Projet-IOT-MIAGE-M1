using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Unsubscribing;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTBroker.Client
{
    public class MQTTClient
    {

        readonly IMqttClient client;
        readonly string id;
        readonly string serverAddr;
        readonly int port;

        public MQTTClient(string id, string serverAddr, int port)
        {

            this.id = id;
            this.serverAddr = serverAddr;
            this.port = port;

            this.client = new MqttFactory().CreateMqttClient();

            this.InitHandler();
  
        }

        private void InitHandler()
        {

            this.client.UseConnectedHandler(e =>
            {
                Console.WriteLine(this.id + " Connected successfully with MQTT Brokers.");
            });

            this.client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine(this.id + " Disconnected from MQTT Brokers.");
            });

            this.client.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine($"### {this.id} RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
            });

        }

        public void Connect()
        {

            var options = new MqttClientOptionsBuilder()
                    .WithClientId(this.id)
                    .WithTcpServer(this.serverAddr, this.port)
                    .WithCredentials("bud", "%spencer%")
                    .WithCleanSession()
                    .Build();

            //connect
            this.client.ConnectAsync(options).Wait();

        }

        public void Disconnect()
        {
            this.client.DisconnectAsync().Wait();
        }

        public void Subscribe(string topic)
        {

            if (!this.client.IsConnected) return;

            var topicMQQT = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();

            this.client.SubscribeAsync(topicMQQT).Wait();
        }

        public void Unsubscribe(string topic)
        {

            if (!this.client.IsConnected) return;

            var topicMQQT = new MqttClientUnsubscribeOptions
            {
                TopicFilters = new List<string>()
            };

            topicMQQT.TopicFilters.Add(topic);

            this.client.UnsubscribeAsync(topicMQQT);
        }

        public void Publish(string topic, string message)
        {

            if (!this.client.IsConnected) return;

            var messageMQQT = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload($"Payload: {message}")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

          
            Console.WriteLine($"publishing at {DateTime.UtcNow}");
            this.client.PublishAsync(messageMQQT);
            
        }

        //TODO: Refactor for better handling
       /* private static void HandleMessage(string payload)
        {
            var messageToPublish = new MqttApplicationMessageBuilder()
                .WithTopic(MY_PUBLISH_TOPIC)
                .WithPayload($"{_printer.PrinterStatus}")
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            switch (payload)
            {
                case "start":
                    _printer.Start();
                    _client.PublishAsync(MY_PUBLISH_TOPIC, Encoding.ASCII.GetBytes($"{_printer.PrinterStatus}")); //*client.PublishAsync(messageToPublish).Wait();
                    break;

                case "stop":
                    _printer.Stop();
                    _client.PublishAsync(MY_PUBLISH_TOPIC, Encoding.ASCII.GetBytes($"{_printer.PrinterStatus}"));
                    break;

                default:
                    _client.PublishAsync(MY_PUBLISH_TOPIC, Encoding.ASCII.GetBytes("UNKNOWN"));
                    break;
            }
        }*/



    }
}
