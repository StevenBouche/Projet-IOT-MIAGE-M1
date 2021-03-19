using Models;
using Models.ViewModels;
using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IOT.Services
{
    public class DataIOTManager : Manager<DataIOT>
    {

        public DataIOTManager(IMongoDBContext<DataIOT> context)
        {
            this.Context = context;
        }

        public List<DataIOT> GetAllDataOfEquipment(string id)
        {
           var test = this.Context.GetQueryable()
                .Where(data => data.EquipmentID == id)
                .OrderBy(data => data.Timestamp).ToList();

            return test;
        }

        public List<DataIOT> Search(SearchDataPeriodView search)
        {
            return this.Context.GetQueryable()
                .Where(data => data.EquipmentID.Equals(search.EquipmentID) && data.Timestamp >= search.TimestampAfter)
                .ToList();
        }
    }
}
