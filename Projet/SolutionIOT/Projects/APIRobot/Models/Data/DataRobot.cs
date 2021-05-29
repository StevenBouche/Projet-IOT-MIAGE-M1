using MongoDBAccess;

namespace APIRobot.Models.Data
{
    public class DataRobot : MongoObject
    {
        public string IdESP { get; set; }
        public double Timestamp { get; set; }
        public double Ligth { get; set; }
        public double Temperature { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
