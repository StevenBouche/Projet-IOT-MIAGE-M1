using MongoDBAccess;

namespace APIRobot.Configs.Database
{
    public class DataDatabaseSettings : IDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
