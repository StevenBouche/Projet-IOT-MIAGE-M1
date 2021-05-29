using APIRobot.Models;
using APIRobot.Models.Data;
using APIRobot.Services.SignalHub;
using MongoDBAccess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIRobot.Services
{
    public class DataRobotService : ManagerMongo<DataRobot>
    {

        private readonly IProxyHubEquipment ProxyHub;

        public DataRobotService(IMongoDBContext<DataRobot> context, IProxyHubEquipment proxyHub)
        {
            Context = context;
            ProxyHub = proxyHub;
        }

        public override DataRobot Create(DataRobot value)
        {
            var data = base.Create(value);

            if (data != null)
                Task.Run(async () => await ProxyHub.OnNewDataRobot(data));

            return data;
        }

        public List<DataRobot> GetAllDataOfEquipment(string id)
        {
            var test = this.Context.GetQueryable()
                 .Where(data => data.IdESP == id)
                 .OrderByDescending(data => data.Timestamp)
                 .ToList();

            return test;
        }

        public List<DataRobot> Search(SearchDataPeriodView search)
        {
            var test = this.Context.GetQueryable()
                .Where(data => data.IdESP.Equals(search.IdEquipment) && data.Timestamp > search.TimestampAfter)
                .OrderBy(data => data.Timestamp)
                .ToList();

            return test;
        }

        public List<DataRobot> SearchLast(SearchDataPeriodView search)
        {
            var test = this.Context.GetQueryable()
                .Where(data => data.IdESP.Equals(search.IdEquipment))
                .OrderByDescending(data => data.Timestamp)
                .Take(search.NbData)
                .ToList();

            return test;
        }

        public List<DataRobot> SearchLast(string id, int size)
        {
            var test = this.Context.GetQueryable()
                .Where(data => data.IdESP.Equals(id))
                .OrderByDescending(data => data.Timestamp)
                .Take(size)
                .OrderBy(data => data.Timestamp)
                .ToList();

            return test;
        }
    }
}
