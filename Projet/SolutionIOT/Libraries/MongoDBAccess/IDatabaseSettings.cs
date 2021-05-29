using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDBAccess
{
    public interface IDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
