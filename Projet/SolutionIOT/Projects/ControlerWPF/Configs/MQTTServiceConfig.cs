

using ControlerWPF.Models.Auth;

namespace ControlerWPF.Configs
{
    public class MQTTServiceConfig
    {
        public string TopicSendControl { get; set; }
        public EquipmentAuth EquipmentAuth { get; set; }
        public string ApiURL { get; set; }
    }
}
