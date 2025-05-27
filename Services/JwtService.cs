using LoanSpa.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace LoanSpa.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration config)
        {
            _configuration = config;
            _secret = config["Jwt:Key"] ?? "YourSuperSecretKeyHereShouldBeLongEnoughForHS256";
            _issuer = config["Jwt:Issuer"] ?? "LoanSpa";
            _audience = config["Jwt:Audience"] ?? "SpaUsers";
            _expiryMinutes = int.Parse(config["Jwt:DurationInMinutes"] ?? "60");
        }

        public string GenerateToken(AspNetUser user, string role = null)
        {
            if (string.IsNullOrEmpty(_secret))
            {
                throw new InvalidOperationException("JWT:Key is missing in configuration");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Sử dụng role được truyền vào nếu có, nếu không thì lấy từ Roles
            string userRole = role ?? "User"; // Mặc định
            if (string.IsNullOrEmpty(role) && user.Roles != null && user.Roles.Any())
            {
                // Lấy tên của role đầu tiên nếu không có role được truyền vào
                userRole = user.Roles.FirstOrDefault()?.Name ?? "User";
            }
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, userRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
