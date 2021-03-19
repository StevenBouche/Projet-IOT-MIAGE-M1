using Models;
using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IOT.Services
{
    public class EquipmentIOTManager : Manager<EquipmentIOT>
    {

        public EquipmentIOTManager(IMongoDBContext<EquipmentIOT> context)
        {
            this.Context = context;
        }

        public EquipmentIOT ReadByEquipmentId(string clientId)
        {
            return this.Context.GetQueryable().FirstOrDefault(equipment => equipment.EquipmentId.Equals(clientId));
        }

        public List<EquipmentIOT> ReadAllConnected()
        {
            return this.Context.GetQueryable().Where(equipment => equipment.IsOnline).ToList();
        }
    }
}
