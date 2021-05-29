using MongoDBAccess;

namespace APIRobot.Configs.Database
{
    public class AuthorizationDatabaseSettings : IDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
