using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIRobot.Models
{
    public class SearchDataPeriodView
    {
        public string IdEquipment { get; set; }
        public long TimestampAfter { get; set; }
        public int NbData { get; set; }

    }
}
