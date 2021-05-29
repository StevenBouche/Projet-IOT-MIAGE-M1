using System;
using System.Security.Claims;

namespace APIRobot.Models.Auth
{
    public class EquipmentIdentity : IEquatable<EquipmentIdentity>
    {
        public string IdEquipment { get; set; }
        public string Role { get; set; }
        public string TypeEquipment { get; set; }

        public EquipmentIdentity()
        {

        }

        public EquipmentIdentity(ClaimsPrincipal claims)
        {
            if (claims != null)
            {
                this.IdEquipment = claims.FindFirstValue(ClaimTypes.NameIdentifier);
                this.Role = claims.FindFirstValue(ClaimTypes.Role);
                this.TypeEquipment = claims.FindFirstValue("TypeEquipment");
            }
        }

        public Claim[] GetClaims()
        {
            return new[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.IdEquipment),
                new Claim(ClaimTypes.Role, this.Role),
                new Claim("TypeEquipment", this.TypeEquipment)
            };
        }

        public override int GetHashCode() => (IdEquipment, Role).GetHashCode();

        public override bool Equals(object other) {

            if (other is EquipmentIdentity)
                return Equals(other as EquipmentIdentity);
            else 
                return false;
        }

        public bool Equals(EquipmentIdentity other)
        {
            return (IdEquipment, Role).Equals((other.IdEquipment, other.Role));
        }
    }

    
}
