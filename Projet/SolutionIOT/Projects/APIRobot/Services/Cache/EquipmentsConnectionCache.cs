using APIRobot.Configs;
using APIRobot.Models;
using APIRobot.Models.Auth;
using APIRobot.Models.Data;
using APIRobot.Services.SignalHub;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using static APIRobot.Services.Cache.ICacheEquipment;

namespace APIRobot.Services.Cache
{

    public class ChannelConnection
    {
        public readonly string IdEquipment;
        public readonly Channel<string> Stream = Channel.CreateUnbounded<string>();
        public readonly Channel<MotorValues> StreamMotor = Channel.CreateUnbounded<MotorValues>();

        public ChannelConnection(string id)
        {
            this.IdEquipment = id;
        }
    }

    public interface ICacheEquipment
    {
       
        List<EquipmentConnection> GetEquipmentConnected();
    }

    public interface IMQTTConnectionCache
    {
        void ConnectedMQTT(EquipmentIdentity value, string idConnection, string addrIP);
        EquipmentValue GetEquipmentMQTT(string idConnection);
        EquipmentValue DisconnectedMQTT(string idConnection);
    }

    public interface ITCPConnectionCache
    {
        void ConnectedTCP(EquipmentIdentity value, string idConnection, string addrIP);
        EquipmentValue DisconnectedTCP(string idConnection);
        EquipmentValue GetEquipmentTCP(string idConnection);
    }

    public interface IChannelConnectionCache
    {
        ChannelConnection GetChannelsEquipment(string idEquipment);
    }

    public class EquipmentsConnectionCache : IMQTTConnectionCache, ITCPConnectionCache, ICacheEquipment, IChannelConnectionCache
    {
        class KeyConnection : IEquatable<KeyConnection>
        {
            public string IdEquipment { get; set; }
            public string TypeEquipment { get; set; }

            public override int GetHashCode() => (IdEquipment, TypeEquipment).GetHashCode();

            public override bool Equals(object other)
            {
                if (other is KeyConnection)
                    return Equals(other as KeyConnection);
                else
                    return false;
            }

            public bool Equals(KeyConnection other)
            {
                return (IdEquipment, TypeEquipment).Equals((other.IdEquipment, other.TypeEquipment));
            }
        }

        private readonly IProxyHubEquipment ProxyHub;
        private readonly ConcurrentDictionary<KeyConnection, EquipmentConnection> Cache;
        private readonly ConcurrentDictionary<KeyConnection, ChannelConnection> CacheChannels;
        private readonly IOptions<EquipmentsConnectionCacheConfig> Config;

        public EquipmentsConnectionCache(IProxyHubEquipment proxyHub, IOptions<EquipmentsConnectionCacheConfig> config)
        {
            Cache = new();
            CacheChannels = new();
            ProxyHub = proxyHub;
            Config = config;
        }

        private static EquipmentValue CreateEquipment(EquipmentIdentity identity, string idConnection, string addrIP)
        {
            return new()
            {
                IdEquipment = identity.IdEquipment,
                IdConnection = idConnection,
                TypeEquipment = identity.TypeEquipment,
                Role = identity.Role,
                AdressIp = addrIP
            };
        }

        public List<EquipmentConnection> GetEquipmentConnected()
        {
            return Cache.Values
                .Where(equipment => Config.Value.TypesEquipments.Any(type => type.Equals(equipment.TypeEquipment)))
                .ToList(); ;
        }

        private void OnChangeEquipmentConnectionEvent(EquipmentConnection equipment)
        {
            if (Config.Value.TypesEquipments.Any(type => type.Equals(equipment.TypeEquipment)))
            {
                Task.Run(async () => await ProxyHub.OnChangeEquipment(equipment));
                Task.Run(async () => await ProxyHub.OnChangeEquipments(GetEquipmentConnected()));
            }
        }

        public void ConnectedMQTT(EquipmentIdentity identity, string idConnection, string addrIP)
        {
            AddConnectionService(ServiceCategory.MQTT, CreateEquipment(identity, idConnection, addrIP));
        }

        public void ConnectedTCP(EquipmentIdentity identity, string idConnection, string addrIP)
        {
            AddConnectionService(ServiceCategory.TCPStream, CreateEquipment(identity, idConnection, addrIP));
        }

        public EquipmentValue DisconnectedMQTT(string idConnection)
        {
            return RemoveConnectionService(ServiceCategory.MQTT, idConnection);
        }

        public EquipmentValue DisconnectedTCP(string idConnection)
        {
            return RemoveConnectionService(ServiceCategory.TCPStream, idConnection);
        }

        public EquipmentValue GetEquipmentMQTT(string idConnection)
        {
            return GetEquipmentService(ServiceCategory.MQTT, idConnection);
        }

        public EquipmentValue GetEquipmentTCP(string idConnection)
        {
            return GetEquipmentService(ServiceCategory.TCPStream, idConnection);
        }

        private void AddConnectionService(ServiceCategory category, EquipmentValue value)
        {

            var key = new KeyConnection()
            {
                IdEquipment = value.IdEquipment,
                TypeEquipment = value.TypeEquipment
            };

            Cache.TryGetValue(key, out EquipmentConnection equipment);

            if(equipment is null)
            {
                equipment = new EquipmentConnection(key.IdEquipment, key.TypeEquipment);
                Cache.TryAdd(key, equipment);
                CacheChannels.TryAdd(key, new ChannelConnection(key.IdEquipment));
            }

            if (!equipment.Equipments.Contains(value))
            {
                value.ServiceCategory = category;
                equipment.Equipments.Add(value);
                OnChangeEquipmentConnectionEvent(equipment);
            }
        }

        private EquipmentValue RemoveConnectionService(ServiceCategory category, string idConnection)
        {

            var value = GetEquipmentService(category, idConnection);

            if (value is not null)
            {

                KeyConnection key = new()
                {
                    IdEquipment = value.IdEquipment,
                    TypeEquipment = value.TypeEquipment
                };

                Cache.TryGetValue(key, out EquipmentConnection equipment);

                if (equipment is not null && equipment.Equipments.Remove(value))
                {
                    if (equipment.Equipments.Count == 0)
                    {
                        Cache.TryRemove(key, out equipment);
                        CacheChannels.TryGetValue(key, out ChannelConnection channels);
                        if(channels is not null)
                        {
                            channels.Stream.Writer.Complete();
                            channels.StreamMotor.Writer.Complete();
                            CacheChannels.TryRemove(key, out channels);
                        }
                    }

                    OnChangeEquipmentConnectionEvent(equipment);
                    return value;
                }
            }
            
            return null;
        }

        private EquipmentValue GetEquipmentService(ServiceCategory category, string idConnection)
        {
            var equipment = Cache.Where(pair => pair.Value.Equipments.Any(elemlist =>
                               elemlist.IdConnection.Equals(idConnection) &&
                               elemlist.ServiceCategory.Equals(category)
                           ))
                           .Select(pair => pair.Value)
                           .FirstOrDefault();

            if (equipment is not null)
                return equipment.Equipments.Find(elem => elem.IdConnection.Equals(idConnection) && elem.ServiceCategory.Equals(category));

            return null;
        }

        public ChannelConnection GetChannelsEquipment(string idEquipment)
        {
            return CacheChannels.Values.FirstOrDefault(channel => channel.IdEquipment.Equals(idEquipment));
        }
    }
}
