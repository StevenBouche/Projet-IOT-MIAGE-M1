using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Unsubscribing;
using MQTTLib.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQTTLib.Client
{
    public class MQTTClient
    {
        readonly IMqttClient Client;
        readonly MQTTConfigClient Config;

        public bool IsConnected
        {
            get => Client.IsConnected;
        }

        public delegate void ConnectedHandler(MqttClientConnectedEventArgs args);
        public delegate void DisconnectedHandler(MqttClientDisconnectedEventArgs args);
        public delegate void MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs args);

        public ConnectedHandler ConnectedEvents { get; set; }
        public DisconnectedHandler DisconnectedEvents { get; set; }
        public MessageReceivedHandler MessageReceiveEvents { get; set; }

        public MQTTClient(MQTTConfigClient config)
        {
            this.Config = config;
            this.Client = new MqttFactory().CreateMqttClient();
        }

        private void InitHandler()
        {
            Client.UseConnectedHandler(e => ConnectedEvents?.Invoke(e));
            Client.UseDisconnectedHandler(e => DisconnectedEvents?.Invoke(e));
            Client.UseApplicationMessageReceivedHandler(e => MessageReceiveEvents?.Invoke(e));
        }

        public Task Connect(string username, string password)
        {

            if (IsConnected)
                return Task.CompletedTask;

            this.InitHandler();

            var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId(Config.ID)
                    .WithTcpServer(Config.Hostname, Config.Port)
                    .WithCleanSession();

            if(username != null && password != null)
            {
                optionsBuilder.WithCredentials(username, password);
            }

            return Client.ConnectAsync(optionsBuilder.Build());

        }

        public Task Disconnect()
        {

            if (!IsConnected)
                return Task.CompletedTask;

            return Client.DisconnectAsync();
        }

        public Task Subscribe(string topic)
        {

            if (!IsConnected) 
                return Task.CompletedTask;

            var topicMQQT = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();

            return Client.SubscribeAsync(topicMQQT);

        }

        public Task Unsubscribe(List<string> topics)
        {

            if (!IsConnected)
                return Task.CompletedTask;

            var topicMQQT = new MqttClientUnsubscribeOptions
            {
                TopicFilters = topics
            };

            return Client.UnsubscribeAsync(topicMQQT);
        }

        public Task Publish(string topic, string message)
        {

            if (!IsConnected) return Task.CompletedTask;

            var messageMQQT = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithAtMostOnceQoS()
                    .WithRetainFlag()
                    .WithQualityOfServiceLevel(0)
                    .Build();

            return Client.PublishAsync(messageMQQT);
        }
    }
}
