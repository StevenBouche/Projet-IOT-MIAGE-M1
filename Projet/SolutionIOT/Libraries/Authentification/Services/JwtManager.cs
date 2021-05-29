using Authentification.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Authentification.Services
{

    public class JwtManager 
    {

        private readonly JwtTokenConfig Config;

        public JwtManager(JwtTokenConfig config)
        {
            this.Config = config;
        }

        public UserIdentity DecodeJwtToken(String token)
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
            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            return new UserIdentity(claims);
        }

        public JwtToken GetJwtToken(UserIdentity identity)
        {
            return new JwtToken
            {
                AccessToken = this.GenerateToken(identity),
                ExpireAt = this.UnixTimeNow() + (this.Config.AccessTokenExpiration * 60)
            };
        }

        public RefreshToken GetRefreshToken(UserIdentity identity)
        {
            return new RefreshToken
            {
                Token = this.GenerateRefreshToken(),
                ExpireAt = this.UnixTimeNow() + (this.Config.RefreshTokenExpiration * 60)
            };
        }

        private string GenerateToken(UserIdentity identity)
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

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }



        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public double UnixTimeNow()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }
}
