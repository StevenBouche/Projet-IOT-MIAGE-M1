using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIRobot.Models.Auth
{
    public class EquipmentAuthView
    {
        public string IdEquipment { get; set; }
        public string Password { get; set; }
        public string TypeEquipment { get; set; }
        public string Role { get; set; }
    }
}
