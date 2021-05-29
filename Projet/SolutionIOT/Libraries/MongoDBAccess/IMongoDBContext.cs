using MongoDB.Driver;
using System.Linq;

namespace MongoDBAccess
{
    public interface IMongoDBContext<T>
    {
        IQueryable<T> GetQueryable();
        IMongoCollection<T> GetCollection();
    }
}
