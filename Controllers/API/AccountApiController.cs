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
using Microsoft.EntityFrameworkCore;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountApiController> _logger;
        private readonly SpaDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountApiController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountApiController> logger,
            SpaDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser { UserName = model.Username, Email = model.Email, PhoneNumber = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Kiểm tra và tạo vai trò "User" nếu chưa tồn tại
                var roleExists = await _roleManager.RoleExistsAsync("User");
                if (!roleExists)
                {
                    var role = new IdentityRole("User");
                    await _roleManager.CreateAsync(role);
                }

                // Gán vai trò "User" cho người dùng
                await _userManager.AddToRoleAsync(user, "User");

                // Tạo Customer mới
                var customer = new Customer
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    Phone = model.PhoneNumber,
                    CreatedDate = DateTime.Now,
                    TotalAmount = 0
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Tạo JWT token
                var token = GenerateJwtToken(user, customer.FullName);

                return Ok(new
                {
                    message = "Đăng ký thành công",
                    token = token,
                    userId = user.Id,
                    customerId = customer.CustomerId,
                    username = user.UserName,
                    fullName = customer.FullName
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

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

                var user = await _userManager.FindByNameAsync(model.Username);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

                var token = GenerateJwtToken(user, customer?.FullName ?? user.UserName);

                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    token = token,
                    userId = user.Id,
                    customerId = customer?.CustomerId,
                    username = user.UserName,
                    fullName = customer?.FullName ?? user.UserName
                });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return StatusCode(423, new { message = "Tài khoản đã bị khóa" });
            }

            return Unauthorized(new { message = "Đăng nhập thất bại, sai tên đăng nhập hoặc mật khẩu" });
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Ok(new { message = "Đăng xuất thành công" });
        }

        private string GenerateJwtToken(IdentityUser user, string fullName)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", fullName)
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