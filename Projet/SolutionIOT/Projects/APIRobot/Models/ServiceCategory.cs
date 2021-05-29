using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIRobot.Models
{
    public class ServiceCategory : IEquatable<ServiceCategory>
    {
        public readonly string NameService;

        private ServiceCategory(string name)
        {
            this.NameService = name;
        }

        public static readonly ServiceCategory MQTT = new("MQTT");
        public static readonly ServiceCategory TCPStream = new("TCPStream");

        public override int GetHashCode() => (NameService).GetHashCode();

        public override bool Equals(object other)
        {
            if (other is ServiceCategory)
            {
                return this.Equals(other as ServiceCategory);
            }
            else return false;
        }

        public bool Equals(ServiceCategory other)
        {
            return (NameService).Equals((other.NameService));
        }
    }
}
