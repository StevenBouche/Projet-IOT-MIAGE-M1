using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPolicy
{
    public class PolicyEntry
    {
        public string Name { get; set; }
        public List<string> Allowed { get; set; }
    }
}
