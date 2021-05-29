using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDBAccess;
using MongoSeeding.Config;
using SharedModels.Auth;
using SharedModels.Authorization;
using System.Threading.Tasks;

namespace MongoSeeding
{
    public class UsersDatabaseSettings : IDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class AuthorizationDatabaseSettings : IDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class App
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<App> _logger;

        public App(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<App>();
            _config = config;
        }

        public async Task Run()
        {
           
            EquipmentsAllowed equipments = new();
            UsersDatabaseSettings settings = new();

            _config.GetSection(nameof(EquipmentsAllowed)).Bind(equipments);
            _config.GetSection(nameof(UsersDatabaseSettings)).Bind(settings);

            MongoDBContext<EquipmentAuth, UsersDatabaseSettings> mongo = new(settings);

            AuthorizationEquipment auth = new();
            AuthorizationDatabaseSettings settingsAuth = new();

            _config.GetSection(nameof(AuthorizationEquipment)).Bind(auth);
            _config.GetSection(nameof(AuthorizationDatabaseSettings)).Bind(settingsAuth);

            MongoDBContext<AuthorizationEquipment, AuthorizationDatabaseSettings> mongoAuth = new(settingsAuth);

            _logger.LogInformation("Delete all documents of collection");
            await mongo.GetCollection().DeleteManyAsync(Builders<EquipmentAuth>.Filter.Empty);
            await mongoAuth.GetCollection().DeleteManyAsync(Builders<AuthorizationEquipment>.Filter.Empty);

            _logger.LogInformation("Insert all documents");
            mongo.GetCollection().InsertMany(equipments.Allowed);
            mongoAuth.GetCollection().InsertOne(auth);
        }
    }
}
