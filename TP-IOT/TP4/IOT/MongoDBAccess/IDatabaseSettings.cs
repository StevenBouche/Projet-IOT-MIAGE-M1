using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDBAccess
{
    public interface IDatabaseSettings
    {
        string GetCollectionName();
        string GetConnectionString();
        string GetDatabaseName();
    }
}
