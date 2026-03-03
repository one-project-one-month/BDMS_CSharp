using BDMS.Database.AppDbContextModels;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Shared.Service
{
    public class TokenService
    {
        private readonly JwtSettings _jwtSettings;
        public TokenService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public(string Token, DateTime Expiration) GenerateToken(User user, string roleName, List<string> permissions)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var encryptedUserId = EncryptionHelper.Encrypt(user.Id.ToString());
            var encryptedEmail = EncryptionHelper.Encrypt(user.Email);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, encryptedUserId),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, encryptedEmail),
                new Claim(ClaimTypes.Role, roleName)
            };

            foreach(var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            if (user.HospitalId.HasValue)
            {
                var encryptedHospitalId = EncryptionHelper.Encrypt(user.HospitalId.Value.ToString());
                claims.Add(new Claim("HospitalId", encryptedHospitalId));
            }

            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes);

            var token = new JwtSecurityToken(
             issuer: _jwtSettings.Issuer,
             audience: _jwtSettings.Audience,
             claims: claims,
             expires: expiration,
             signingCredentials: credentials
             );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }
    }
}
