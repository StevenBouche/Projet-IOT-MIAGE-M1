using MongoDB.Driver;
using System.Linq;

namespace MongoDBAccess
{
    public class MongoDBContext<T, K> : IMongoDBContext<T> where K : IDatabaseSettings
    {

        private K Settings;
        private readonly MongoClient Client;
        private readonly IMongoDatabase Database;

        public MongoDBContext(K settings)
        {
            this.Settings = settings;
            this.Client = new MongoClient(this.Settings.GetConnectionString());
            this.Database = this.Client.GetDatabase(this.Settings.GetDatabaseName());
        }

        public IQueryable<T> GetQueryable()
        {
            return this.GetCollection().AsQueryable();
        }

        public IMongoCollection<T> GetCollection()
        {
            return this.Database.GetCollection<T>(this.Settings.GetCollectionName());
        }

    }
}
