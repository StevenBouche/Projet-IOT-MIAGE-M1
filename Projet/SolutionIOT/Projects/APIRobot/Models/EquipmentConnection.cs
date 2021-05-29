using System;
using System.Collections.Generic;

namespace APIRobot.Models
{

    public class EquipmentConnection
    {
        public string IdEquipment { get; set; }
        public string TypeEquipment { get; set; }
        public List<EquipmentValue> Equipments { get; set; }

        public EquipmentConnection()
        {

        }

        public EquipmentConnection(string id, string type)
        {
            this.IdEquipment = id;
            this.TypeEquipment = type;
            this.Equipments = new List<EquipmentValue>();
        }
    }
}
