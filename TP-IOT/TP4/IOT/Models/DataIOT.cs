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
    public class DataIOT : MongoObject
    {

        public string EquipmentID { get; set; }
        public long Ligth { get; set; }
        public long Temperature { get; set; }
        public long Timestamp { get; set; }

    }
}
