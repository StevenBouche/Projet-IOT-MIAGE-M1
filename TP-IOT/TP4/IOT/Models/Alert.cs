using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Alert : MongoObject
    {

        public string EquipmentId { get; set; }
        public long Temperature { get; set; }
        public long Timestamp { get; set; }
        public bool IsHandle { get; set; }

    }
}
