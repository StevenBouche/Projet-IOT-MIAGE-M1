using MongoDBAccess;
using System.Collections.Generic;

namespace SharedModels.Auth
{
    public class EquipmentAuth : MongoObject
    {
        public string IdEquipment { get; set; }
        public string TypeEquipment { get; set; }
        public string Password { get; set; }
        public List<EquipmentRole> Role { get; set; }
    }
}
