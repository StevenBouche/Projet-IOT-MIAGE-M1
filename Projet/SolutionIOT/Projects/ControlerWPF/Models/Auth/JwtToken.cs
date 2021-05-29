using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlerWPF.Models.Auth
{
    public class JwtToken
    {
        public string AccessToken { get; set; }

        public double ExpireAt { get; set; }
    }
}
