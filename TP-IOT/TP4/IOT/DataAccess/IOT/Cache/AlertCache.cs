using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IOT.Cache
{
    public class AlertCache
    {

        private List<Alert> Cache { get; set; }

        private readonly object Lock = new object();

        public AlertCache()
        {
            this.Cache = new List<Alert>();
        }

        public Alert GetFirstAlertNotHandleOfEquipmentById(string id)
        {
            return this.Cache.FirstOrDefault(alert => alert.EquipmentId.Equals(id) && !alert.IsHandle);
        }

        public void ClearAndAdd(List<Alert> alerts)
        {

            lock (Lock)
            {
                this.Cache.Clear();
                this.Cache = alerts;
            }

        }

        public List<Alert> SetAlertAndReturnNew(List<Alert> alerts)
        {

            List<Alert> news = alerts.Where(alert => !this.Cache.Any(alert2 => alert2.Id.Equals(alert.Id))).ToList();

            this.ClearAndAdd(alerts);

            return news;

        }

        public void Add(Alert alert)
        {
            this.Cache.Add(alert);
        }

        public void Remove(Alert alert)
        {
            this.Cache.RemoveAll(a => a.Id.Equals(alert.Id));
        }
    }
}
