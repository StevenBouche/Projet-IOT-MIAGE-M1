using APIRobot.Models.Auth;
using MongoDBAccess;
using SharedModels.Auth;
using System.Linq;

namespace APIRobot.Services
{
    public class UsersService : ManagerMongo<EquipmentAuth>
    {

        public UsersService(IMongoDBContext<EquipmentAuth> Context)
        {
            this.Context = Context;
        }

        public EquipmentAuth ReadByIdEquipment(string id)
        {
            return this.Context.GetQueryable().FirstOrDefault(element => element.IdEquipment.Equals(id));
        }
    }
}
