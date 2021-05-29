using ControlerWPF.Configs;
using ControlerWPF.Models;
using ControlerWPF.Models.Auth;
using MQTTLib.Client;
using MQTTLib.Config;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MQTTLib.Client.MQTTClient;

namespace ControlerWPF.Services
{
    public class MQTTService
    {
        
        public ConnectedHandler ConnectedEvents { get; set; }
        public DisconnectedHandler DisconnectedEvents { get; set; }

        private readonly MQTTClient Client;
        private readonly MQTTConfigClient ConfigClient;
        private readonly MQTTServiceConfig ConfigTopic;
        private long LastSend;
        private readonly long DelaySendMilliSeconds = 1000;
        private readonly HttpClient HttpClient;

        public bool IsConnected
        {
            get => Client.IsConnected;
        }

        public MQTTService(MQTTConfigClient config, MQTTServiceConfig topicConfig)
        {

            ConfigClient = config;
            ConfigTopic = topicConfig;
            HttpClient = new HttpClient();
            Client = new MQTTClient(this.ConfigClient)
            {
                ConnectedEvents = OnConnect,
                DisconnectedEvents = OnDisconnect
            };
        }

        private void OnDisconnect(MqttClientDisconnectedEventArgs args)
        {
            DisconnectedEvents?.Invoke(args);
        }

        private void OnConnect(MqttClientConnectedEventArgs args)
        {
            ConnectedEvents?.Invoke(args);
        }

        public async Task<Task> Connect()
        {

            var json = JsonConvert.SerializeObject(this.ConfigTopic.EquipmentAuth);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(this.ConfigTopic.ApiURL, data);
            string result = response.Content.ReadAsStringAsync().Result;

            var jwt = JsonConvert.DeserializeObject<JwtToken>(result);

            return this.Client.Connect(jwt.AccessToken, "");
        }

        public Task Disconnect()
        {
            return this.Client.Disconnect();
        }

        public Task SendDataMotors(MotorValues message)
        {

           /* long current = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if(current < LastSend+DelaySendMilliSeconds)
            {
                return Task.CompletedTask;
            }

            LastSend = current;*/

            return this.Client.Publish(ConfigTopic.TopicSendControl, JsonConvert.SerializeObject(message));
        }
    }
}
