using APIRobot.Configs;
using APIRobot.Models.Auth;
using Authentification.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedModels.Auth;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace APIRobot.Services
{
    public interface IValidatorEquipmentToken
    {
        EquipmentIdentity DecodeJwtToken(string token);
    }

    public class JwtService : IValidatorEquipmentToken
    {

        private readonly JwtServiceConfig Config;
        private readonly UsersService Service;

        public JwtService(IOptions<JwtServiceConfig> config, UsersService Service)
        {
            this.Service = Service;
            this.Config = config.Value;
        }

        public EquipmentIdentity DecodeJwtToken(string token)
        {
            
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidIssuer = this.Config.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(this.Config.Secret)),
                ValidAudience = this.Config.Audience,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(this.Config.AccessTokenExpiration)
            };

            ClaimsPrincipal claims;
            try
            {
                claims = handler.ValidateToken(token, validations, out var tokenSecure);
            }
            catch (Exception)
            {
                return null;
            }
           
            return new EquipmentIdentity(claims);
        }

        public JwtToken GetJwtToken(EquipmentAuthView value)
        {
            EquipmentAuth eq = this.Service.ReadByIdEquipment(value.IdEquipment);

            if (eq is null)
                return null;

            if (!eq.Password.Equals(value.Password))
                return null;

            if (!eq.TypeEquipment.Equals(value.TypeEquipment))
                return null;

            if (!eq.Role.Any(role => role.Name.Equals(value.Role)))
                return null;

            EquipmentIdentity identity = new()
            {
                IdEquipment = eq.IdEquipment,
                Role = value.Role,
                TypeEquipment = eq.TypeEquipment
            };

            return new JwtToken
            {
                AccessToken = this.GenerateToken(identity),
                ExpireAt = UnixTimeNow() + (this.Config.AccessTokenExpiration * 60)
            };
        }

        private string GenerateToken(EquipmentIdentity identity)
        {

            var claims = identity.GetClaims();

            byte[] key = Convert.FromBase64String(this.Config.Secret);
            SymmetricSecurityKey securityKey = new(key);
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(this.Config.RefreshTokenExpiration * 60),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        private static double UnixTimeNow()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }
}
