using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Authorization
{
    public class ElementAuthorizationConfig
    {
        public string AuthorizeTypeEquipment { get; set; }
        public List<string> AuthorizeRole { get; set; }
    }

    public class MQTTTopicsAuthorizationConfig
    {
        public string TopicName { get; set; }
        public int MaxTerms { get; set; }
        public List<ElementAuthorizationConfig> AuthorizeSending { get; set; }
        public List<ElementAuthorizationConfig> AuthorizeSubscribe { get; set; }
    }

    public class ConnectionAuthorizationConfig
    {
        public List<ElementAuthorizationConfig> AuthorizeConnection { get; set; }
    }

    public class AuthorizationEquipment : MongoObject
    {
        public string Version { get; set; }
        public List<MQTTTopicsAuthorizationConfig> TopicsAuthozizationConfigs { get; set; }
        public ConnectionAuthorizationConfig StreamAuthozizationConnectionConfigs { get; set; }
        public ConnectionAuthorizationConfig MQTTAuthozizationConnectionConfigs { get; set; }
    }
}
