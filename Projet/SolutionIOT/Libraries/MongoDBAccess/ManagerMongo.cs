using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace MongoDBAccess
{
    public abstract class ManagerMongo<T> where T : MongoObject
    {

        protected IMongoDBContext<T> Context;

        public virtual T Create(T value)
        {
            this.Context.GetCollection().InsertOne(value);
            return this.Read(value);
        }

        public T Read(T value)
        {
            return this.ReadById(value.Id);
        }

        public T ReadById(string id)
        {
            if (!string.IsNullOrEmpty(id))
                return this.Context.GetQueryable().FirstOrDefault(element => element.Id.Equals(id));
            else
                return null;
        }

        public List<T> ReadAll()
        {
            return this.Context.GetQueryable().ToList();
        }

        public T Update(T value)
        {
            this.Context.GetCollection().ReplaceOne(element => element.Id == value.Id, value);
            return this.Read(value);
        }

        public bool Delete(T value)
        {
            return this.DeleteById(value.Id);
        }

        public bool DeleteById(string id)
        {
            return this.Context.GetCollection()
               .DeleteOne(element => element.Id == id)
               .DeletedCount == 1;
        }

 

    }
}
