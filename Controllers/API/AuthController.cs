using LoanSpa.Data;
using LoanSpa.Models;
using LoanSpa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService,
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager)
        {
            _context = context;
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập không tồn tại" });
            }
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Mật khẩu không đúng" });
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles.FirstOrDefault() ?? "User");
            
            // Lấy thông tin khách hàng nếu có
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            
            return Ok(new AuthResponse
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = customer?.FullName,
                Role = roles.FirstOrDefault() ?? "User",
                CustomerId = customer?.CustomerId,
                Token = token
            });
        }
        
        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            // Kiểm tra username đã tồn tại chưa
            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
            }
            
            // Kiểm tra email đã tồn tại chưa
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return BadRequest(new { message = "Email đã được sử dụng" });
            }
            
            // Tạo user mới
            var user = new AspNetUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true // Trong thực tế nên xác nhận email
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Đăng ký thất bại", errors = result.Errors });
            }
            
            // Gán role User cho user mới
            await _userManager.AddToRoleAsync(user, "User");
            
            // Tạo thông tin khách hàng
            var customer = new Customer
            {
                UserId = user.Id,
                FullName = model.FullName,
                Phone = model.PhoneNumber,
                CreatedDate = DateTime.Now
            };
            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            
            // Tạo token JWT
            var token = _jwtService.GenerateToken(user, "User");
            
            return Ok(new AuthResponse
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = customer.FullName,
                Role = "User",
                CustomerId = customer.CustomerId,
                Token = token
            });
        }
        
        // GET: api/Auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            var roles = await _userManager.GetRolesAsync(user);
            
            // Không trả về password
            return Ok(new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = customer?.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "User",
                CustomerId = customer?.CustomerId
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
