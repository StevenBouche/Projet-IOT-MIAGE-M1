using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Authentification.Models
{
    public class LoginResult
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("jwtToken")]
        public JwtToken JwtToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public RefreshToken RefreshToken { get; set; }
    }
}
