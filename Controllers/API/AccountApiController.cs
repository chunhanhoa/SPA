using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LoanSpa.Data;
using LoanSpa.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountApiController> _logger;
        private readonly SpaDbContext _context; // Thêm context để làm việc với db
        private readonly IConfiguration _configuration; // Thêm để đọc JWT secret từ cấu hình

        public AccountApiController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountApiController> logger,
            SpaDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        // POST: api/AccountApi/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                
                // Tạo Customer mới và lưu vào database
                var customer = new Customer
                {
                    UserId = user.Id,
                    FullName = model.Username, // Có thể thêm trường fullName vào RegisterViewModel
                    Phone = "",  // Có thể thêm trường phone vào RegisterViewModel
                    CreatedDate = DateTime.Now,
                    TotalAmount = 0
                };
                
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                
                // Tạo JWT token
                var token = GenerateJwtToken(user);
                
                return Ok(new { 
                    message = "Đăng ký thành công", 
                    token = token,
                    userId = user.Id,
                    customerId = customer.CustomerId
                });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return BadRequest(ModelState);
        }

        // POST: api/AccountApi/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                // Lấy user và customer
                var user = await _userManager.FindByNameAsync(model.Username);
                var customer = _context.Customers.FirstOrDefault(c => c.UserId == user.Id);
                
                // Tạo JWT token
                var token = GenerateJwtToken(user);
                
                return Ok(new { 
                    message = "Đăng nhập thành công", 
                    token = token,
                    userId = user.Id,
                    customerId = customer?.CustomerId
                });
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return StatusCode(423, new { message = "Tài khoản đã bị khóa" });
            }
            
            return Unauthorized(new { message = "Đăng nhập thất bại, sai tên đăng nhập hoặc mật khẩu" });
        }

        // POST: api/AccountApi/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Ok(new { message = "Đăng xuất thành công" });
        }
        
        // Hàm tạo JWT token
        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "LoanSpaSecretKey123456789012345678"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"] ?? "https://localhost:5099",
                audience: _configuration["JWT:ValidAudience"] ?? "https://localhost:5099",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
