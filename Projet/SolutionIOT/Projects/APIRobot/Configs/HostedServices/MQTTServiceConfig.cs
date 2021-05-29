
namespace APIRobot.Configs.HostedServices
{
    public class MQTTConfigBroker
    {
        public int Port { get; set; }
        public int EncryptedPort { get; set; }
    }

    public class MQTTServiceConfig
    {
        public MQTTConfigBroker MQTTConfigBroker { get; set; }
        public string TopicNameDataRobot { get; set; }
        public string TopicNameMotorDataRobot { get; set; }
    }
}