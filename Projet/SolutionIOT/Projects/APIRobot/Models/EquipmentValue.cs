using System;

namespace APIRobot.Models
{
    public class EquipmentValue : IEquatable<EquipmentValue>
    {
        public ServiceCategory ServiceCategory;

        public string IdEquipment { get; set; }
        public string IdConnection { get; set; }
        public string ServiceName { get => ServiceCategory.NameService; }
        public string TypeEquipment { get; set; }
        public string Role { get; set; }
        public string AdressIp { get; set; }

        public override int GetHashCode() => (IdEquipment, IdConnection, ServiceCategory, Role, AdressIp).GetHashCode();

        public override bool Equals(object other)
        {
            if (other is EquipmentValue)
                return Equals(other as EquipmentValue);
            else 
                return false;
        }

        public bool Equals(EquipmentValue other)
        {
            return (IdEquipment, IdConnection, ServiceCategory, Role, AdressIp)
                .Equals((other.IdEquipment, other.IdConnection, other.ServiceCategory, other.Role, other.AdressIp));
        }
    }
}
