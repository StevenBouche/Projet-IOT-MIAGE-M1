using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class SearchDataPeriodView
    {

        public string EquipmentID { get; set; } 
        public long TimestampAfter { get; set; }
        public int NbData { get; set; }

    }
}
