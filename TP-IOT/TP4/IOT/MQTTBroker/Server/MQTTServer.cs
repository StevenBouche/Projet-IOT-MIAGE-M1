using DataAccess.IOT;
using DataAccess.IOT.Cache;
using DataAccess.IOT.Services;
using Models;
using MongoDBAccess;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MQTTBroker.Server
{
    class DataClient
    {
        public long Temperature { get; set; }
        public long Ligth { get; set; }
    }

    class MessageAlert
    {
        public string ClientId { get; set; }
    }

    public class MQTTServer
    {
        //https://www.youtube.com/watch?v=PSerr2fvnyc&ab_channel=Industry40tv

        static readonly int ConnectionBacklog = 100;
        readonly int Port = 1883;
        readonly string IpAddr = "127.0.0.1";
        static readonly string DATA_TOPIC = "IOT/data";
        static readonly string ALERT_HANDLE_TOPIC = "IOT/alertHandle";
        static readonly string ALERT_TOPIC = "IOT/alert";

        static readonly string Username = "clientProjectIOTMIAGE";
        static readonly string Password = "XAjNyUPfS8FmBQ[s";
        static readonly int SeuilAlertTemp = 30;

        readonly AlertCache Cache = new AlertCache();
        
        readonly IMqttServer server;

        DataIOTManager DataManager;
        EquipmentIOTManager EquipmentManager;
        AlertManager AlertManager;
        
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
            IDatabaseSettings settingData = new DataIOTDatabaseSetting
            {
                CollectionName = "IOTData",
                ConnectionString = "mongodb://mongoIOT:27017",
                DatabaseName = "IOT"
            };

            IDatabaseSettings settingEquipment = new EquipmentIOTDatabaseSetting
            {
                CollectionName = "IOTEquipment",
                ConnectionString = "mongodb://mongoIOT:27017",
                DatabaseName = "IOT"
            };

            IDatabaseSettings settingAlert = new AlertDatabaseSetting
            {
                CollectionName = "Alert",
                ConnectionString = "mongodb://mongoIOT:27017",
                DatabaseName = "IOT"
            };

            IMongoDBContext<DataIOT> contextData = new MongoDBContext<DataIOT, IDatabaseSettings>(settingData);
            IMongoDBContext<EquipmentIOT> contextEquipment = new MongoDBContext<EquipmentIOT, IDatabaseSettings>(settingEquipment);
            IMongoDBContext<Alert> contextAlert = new MongoDBContext<Alert, IDatabaseSettings>(settingAlert);

            this.DataManager = new DataIOTManager(contextData);
            this.EquipmentManager = new EquipmentIOTManager(contextEquipment);
            this.AlertManager = new AlertManager(contextAlert);

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

            string topic = e.ApplicationMessage.Topic;
            string id = e.ClientId;

            if (topic.Equals(DATA_TOPIC))
            {

                long timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

                try
                {
                    DataClient data = JsonConvert.DeserializeObject<DataClient>(e.ApplicationMessage.ConvertPayloadToString());

                    var newData = new DataIOT
                    {
                        EquipmentID = id,
                        Timestamp = timestamp,
                        Temperature = data.Temperature,
                        Ligth = data.Ligth
                    };

                    newData = this.DataManager.Create(newData);

                   /* if (data.Temperature > SeuilAlertTemp)
                    {
                        Alert alert = this.Cache.GetFirstAlertNotHandleOfEquipmentById(id);

                        if (alert is null)
                        {
                            alert = new Alert
                            {
                                EquipmentId = id,
                                Temperature = data.Temperature,
                                IsHandle = false,
                                Timestamp = timestamp
                            };

                            alert = this.AlertManager.Create(alert);

                            this.Cache.Add(alert);

                           var msgAlert = new MessageAlert
                            {
                                ClientId = id
                            };

                            MqttApplicationMessage message = new MqttApplicationMessage();
                            message.Topic = ALERT_TOPIC;
                            message.ContentType = "application/json";
                            message.Payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgAlert));

                            this.server.PublishAsync(message);

                        }

                    }*/

                }
                catch(JsonReaderException ex)
                {
                    Console.WriteLine(ex.Message);
                }
          
            }
            else if (topic.Equals(ALERT_HANDLE_TOPIC))
            {

                Alert alert = this.Cache.GetFirstAlertNotHandleOfEquipmentById(id);

                if(alert is not null)
                {

                    alert.IsHandle = true;

                    alert = this.AlertManager.Update(alert);

                    this.Cache.Remove(alert);

                }

            }

        }

        private async void HandlerClientConnection(MqttServerClientConnectedEventArgs e)
        {

            string id = e.ClientId;

            Console.WriteLine($"{id} connected to broker.");

            string addrIp = await this.GetAddrIpClient(id);
            long timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

            EquipmentIOT eq = this.EquipmentManager.ReadByEquipmentId(id);
            
            if (eq is null)
            {
                eq = new EquipmentIOT
                {
                    EquipmentId = id,
                    IsOnline = true,
                    LastConnectionTimestamp = timestamp,
                    AdressIP = addrIp
                };

                eq = this.EquipmentManager.Create(eq);
            } 
            else
            {
                eq.AdressIP = addrIp;
                eq.IsOnline = true;
                eq.LastConnectionTimestamp = timestamp;

                eq = this.EquipmentManager.Update(eq);
            }

        }

        private void HandlerClientDisconect(MqttServerClientDisconnectedEventArgs e)
        {
            string id = e.ClientId;

            Console.WriteLine($"{id} disconnected to broker.");

            EquipmentIOT eq = this.EquipmentManager.ReadByEquipmentId(id);

            if (eq is not null)
            {

                eq.IsOnline = false;

                eq = this.EquipmentManager.Update(eq);

            }
            
        }

        private void HandlerClientUnsubscribed(MqttServerClientSubscribedTopicEventArgs e)
        {
            Console.WriteLine($"{e.ClientId} sub to topic {e.TopicFilter}.");
        }

        private void HandlerClientSubscribed(MqttServerClientUnsubscribedTopicEventArgs e)
        {
            Console.WriteLine($"{e.ClientId} unsub to topic {e.TopicFilter}.");
        }

        private async Task<string> GetAddrIpClient(string id)
        {
            IList<IMqttClientStatus> Csl = await server.GetClientStatusAsync();
            IMqttClientStatus Cs = Csl.FirstOrDefault(c => c.ClientId == id);
            return Cs is null ? "" : Cs.Endpoint;
        }
      

    }

}
