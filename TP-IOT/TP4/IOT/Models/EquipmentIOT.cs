using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class EquipmentIOT : MongoObject
    {

        public string EquipmentId { get; set; }
        public bool IsOnline { get; set; }
        public string AdressIP { get; set; }
        public long LastConnectionTimestamp { get; set; }

    }
}
