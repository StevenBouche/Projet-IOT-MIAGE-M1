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
                .OrderByDescending(data => data.Timestamp)
                .ToList();

            return test;
        }

        public List<DataIOT> Search(SearchDataPeriodView search)
        {
            var test = this.Context.GetQueryable()
                .Where(data => data.EquipmentID.Equals(search.EquipmentID) && data.Timestamp > search.TimestampAfter)
                .OrderBy(data => data.Timestamp)
                .ToList();

            return test;
        }

        public List<DataIOT> SearchLast(SearchDataPeriodView search)
        {
            var test = this.Context.GetQueryable()
                .Where(data => data.EquipmentID.Equals(search.EquipmentID))
                .OrderByDescending(data => data.Timestamp)
                .Take(search.NbData)
                .OrderBy(data => data.Timestamp)
                .ToList();

            return test;
        }
    }
}
