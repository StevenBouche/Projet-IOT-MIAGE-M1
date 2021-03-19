using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IOT.Services
{
    public abstract class Manager<T> where T : MongoObject
    {

        protected IMongoDBContext<T> Context;

        public T Create(T value)
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
            if (string.IsNullOrEmpty(id))
                return this.ReadById(id);
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
