using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QL_Spa.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, UserManager<IdentityUser> userManager, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<string> CreateTokenAsync(IdentityUser user)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? "")
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["JwtSettings:SecretKey"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.Now.AddDays(Convert.ToDouble(
                    _configuration["JwtSettings:ExpirationDays"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials
                );

                string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
                
                // Lưu token vào AspNetUserTokens
                await SaveTokenToDatabase(user, jwtToken, expires);
                
                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo JWT token");
                throw;
            }
        }
        
        private async Task SaveTokenToDatabase(IdentityUser user, string token, DateTime expiry)
        {
            try
            {
                // Xóa token cũ nếu có
                await _userManager.RemoveAuthenticationTokenAsync(
                    user, 
                    "JwtProvider", // LoginProvider
                    "JwtToken" // Name
                );
                
                // Thêm token mới
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    "JwtProvider", // LoginProvider 
                    "JwtToken", // Name
                    token // Token value
                );
                
                // Tạo thêm token expiry để lưu thời gian hết hạn
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    "JwtProvider",
                    "JwtExpiry",
                    expiry.ToString("o") // Lưu thời gian hết hạn ở định dạng ISO 8601
                );
                
                _logger.LogInformation("Đã lưu JWT token vào database cho user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu JWT token vào database cho user: {UserId}", user.Id);
                // Không throw exception ở đây để tránh ảnh hưởng đến luồng xác thực
            }
        }
        
        // Thêm phương thức để xóa token khi đăng xuất
        public async Task RemoveTokenAsync(IdentityUser user)
        {
            try 
            {
                await _userManager.RemoveAuthenticationTokenAsync(
                    user, 
                    "JwtProvider", 
                    "JwtToken"
                );
                
                await _userManager.RemoveAuthenticationTokenAsync(
                    user, 
                    "JwtProvider", 
                    "JwtExpiry"
                );
                
                _logger.LogInformation("Đã xóa JWT token từ database cho user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa JWT token từ database cho user: {UserId}", user.Id);
            }
        }
        
        // Thêm phương thức để kiểm tra token từ database
        public async Task<string> GetTokenFromDatabaseAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return null;
                
                return await _userManager.GetAuthenticationTokenAsync(
                    user,
                    "JwtProvider",
                    "JwtToken"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy JWT token từ database cho user: {UserId}", userId);
                return null;
            }
        }
    }
}
