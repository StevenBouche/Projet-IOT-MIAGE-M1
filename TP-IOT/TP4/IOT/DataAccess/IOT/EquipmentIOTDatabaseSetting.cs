using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IOT
{
    public interface IEquipmentIOTDatabaseSetting : IDatabaseSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }

    public class EquipmentIOTDatabaseSetting : IEquipmentIOTDatabaseSetting
    {
        public string CollectionName { get ; set; }
        public string ConnectionString { get; set ; }
        public string DatabaseName { get ; set ; }

        public string GetCollectionName()
        {
            return this.CollectionName;
        }

        public string GetConnectionString()
        {
            return this.ConnectionString;
        }

        public string GetDatabaseName()
        {
            return this.DatabaseName;
        }
    }

}
