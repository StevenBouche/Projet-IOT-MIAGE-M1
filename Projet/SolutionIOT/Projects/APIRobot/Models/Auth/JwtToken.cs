using System.Text.Json.Serialization;

namespace Authentification.Models
{
    public class JwtToken
    {
        public string AccessToken { get; set; }

        public double ExpireAt { get; set; }
    }
}
