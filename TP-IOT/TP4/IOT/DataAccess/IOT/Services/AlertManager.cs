using Models;
using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IOT.Services
{
    public class AlertManager : Manager<Alert>
    {

        public AlertManager(IMongoDBContext<Alert> contextAlert)
        {
            this.Context = contextAlert;
        }
    }
}
