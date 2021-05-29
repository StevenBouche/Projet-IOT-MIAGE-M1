using APIRobot.Configs;
using APIRobot.Models;
using APIRobot.Models.Auth;
using Microsoft.Extensions.Options;
using MongoDBAccess;
using SharedModels.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APIRobot.Services
{

    public interface IAuthorizationMQTT
    {
        bool VerifyMQTTAuthorizationConnection(string token, out EquipmentIdentity identity);
        bool VerifyAuthorizationRoleSubscribeTopic(EquipmentValue equipment, string topicName);
        bool VerifyAuthorizationRoleSenderTopic(EquipmentValue equipment, string topicName);
        bool TopicsMatch(string topicMain, string topicValue);
    }

    public interface IAuthorizationTCP
    {
        bool VerifyTCPAuthorizationConnection(string token, out EquipmentIdentity identity);
    }

    public class AuthorizationService : IAuthorizationMQTT, IAuthorizationTCP
    {

        enum ServiceTypeAuthorization
        {
            MQTT,
            TCP
        }

        private readonly AuthorizationServiceConfig Config;
        private readonly IValidatorEquipmentToken ServiceAuth;
        private readonly IMongoDBContext<AuthorizationEquipment> Context;
        private readonly AuthorizationEquipment Authorizations;

        public AuthorizationService(IMongoDBContext<AuthorizationEquipment> context, IValidatorEquipmentToken serviceAuth, IOptions<AuthorizationServiceConfig> config)
        {
            ServiceAuth = serviceAuth;
            Config = config.Value;
            Context = context;
            Authorizations = GetAuthorizations();
        }

        public bool VerifyMQTTAuthorizationConnection(string token, out EquipmentIdentity identity)
        {
            if (!VerifyAuthorizationConnection(token, out identity))
                return false;

            var role = identity.Role;
            var type = identity.TypeEquipment;
            var auth = GetAuthorizationsConnection(ServiceTypeAuthorization.MQTT);

            return auth.Any(auth => VerifyTypeAndRole(auth, type, role));
        }

        public bool VerifyTCPAuthorizationConnection(string token, out EquipmentIdentity identity)
        {
            if(!VerifyAuthorizationConnection(token, out identity))
                return false;

            var role = identity.Role;
            var type = identity.TypeEquipment;
            var auth = GetAuthorizationsConnection(ServiceTypeAuthorization.TCP);

            return auth.Any(auth => VerifyTypeAndRole(auth, type, role));
        }

        private bool VerifyAuthorizationConnection(string token, out EquipmentIdentity identity)
        {
            identity = null;

            if (string.IsNullOrEmpty(token))
                return false;

            identity = ServiceAuth.DecodeJwtToken(token);
            return identity is not null;
        }

        public bool VerifyAuthorizationRoleSenderTopic(EquipmentValue equipment, string topicName)
        {
            return VerifyAuthorizationRoleTopic(
                (config) => config.AuthorizeSending,
                equipment,
                topicName
            );
        }

        public bool VerifyAuthorizationRoleSubscribeTopic(EquipmentValue equipment, string topicName)
        {
            return VerifyAuthorizationRoleTopic(
                (config) => config.AuthorizeSubscribe,
                equipment,
                topicName
            );
        }

        private bool VerifyAuthorizationRoleTopic(Func<MQTTTopicsAuthorizationConfig, List<ElementAuthorizationConfig>> selectList, EquipmentValue equipment, string topic)
        {
            if (equipment is null || string.IsNullOrEmpty(topic))
                return false;

            //topicConf.TopicName.Equals(topic)
            var config = GetAuthorizationsTopics().FirstOrDefault(topicConf => TopicsMatch(topicConf, topic));

            return config is not null &&
                selectList(config) is not null &&
                selectList(config).Any(auth => VerifyTypeAndRole(auth, equipment.TypeEquipment, equipment.Role));
        }

        public bool TopicsMatch(string topicMain, string topicValue)
        {
            var config = GetAuthorizationsTopics().FirstOrDefault(topicConf => topicConf.TopicName.Equals(topicMain));
            return TopicsMatch(config, topicValue);
        }

        private static bool TopicsMatch(MQTTTopicsAuthorizationConfig config, string topicValue)
        {
            string[] termTopicConf = config.TopicName.Split('/');
            string[] termTopicValue = topicValue.Split('/');

            if (termTopicConf.Length != 0 && termTopicConf.Length < termTopicValue.Length && termTopicValue.Length > config.MaxTerms)
                return false;

            for(int i = 0; i < termTopicConf.Length; i++)
            {
                if (!termTopicConf[i].Equals(termTopicValue[i]))
                    return false;
            }

            return true;
        }

        private static bool VerifyTypeAndRole(ElementAuthorizationConfig element, string type, string role)
        {
            return element is not null && 
                element.AuthorizeTypeEquipment is not null &&
                element.AuthorizeRole is not null &&
                element.AuthorizeTypeEquipment.Equals(type) && 
                element.AuthorizeRole.Contains(role);
        }

        private List<ElementAuthorizationConfig> GetAuthorizationsConnection(ServiceTypeAuthorization type)
        {

            if (Authorizations is not null)
            {
                if (type == ServiceTypeAuthorization.MQTT)
                    return Authorizations.MQTTAuthozizationConnectionConfigs.AuthorizeConnection;
                else if (type == ServiceTypeAuthorization.TCP)
                    return Authorizations.StreamAuthozizationConnectionConfigs.AuthorizeConnection;
            }
            
            return new List<ElementAuthorizationConfig>();
        }

        private List<MQTTTopicsAuthorizationConfig> GetAuthorizationsTopics()
        {

            if (Authorizations is not null)
            {
                return Authorizations.TopicsAuthozizationConfigs;
            }

            return new List<MQTTTopicsAuthorizationConfig>();
        }

        private AuthorizationEquipment GetAuthorizations()
        {
            return Context.GetQueryable()
                .FirstOrDefault(element => element.Version.Equals(Config.UsedVersion));
        }
    }
}
