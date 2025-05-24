using LoanSpa.Models;
using LoanSpa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        
        // Danh sách users giả lập (trong thực tế bạn sẽ dùng database)
        private static readonly List<AppUser> _users = new List<AppUser>
        {
            new AppUser
            {
                Id = "1",
                Username = "admin",
                Email = "admin@loanspa.com",
                Password = "admin123", // Trong thực tế, lưu hash của password
                FullName = "Admin",
                PhoneNumber = "0123456789",
                Role = "Admin"
            },
            new AppUser
            {
                Id = "2",
                Username = "user",
                Email = "user@example.com",
                Password = "user123", // Trong thực tế, lưu hash của password
                FullName = "Khách Hàng",
                PhoneNumber = "0987654321",
                Role = "User"
            }
        };
        
        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }
        
        // POST: api/Auth/login
        [HttpPost("login")]
        public IActionResult Login(AuthRequest model)
        {
            var user = _users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });
            }
            
            var token = _jwtService.GenerateToken(user);
            
            return Ok(new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token
            });
        }
        
        // POST: api/Auth/register
        [HttpPost("register")]
        public IActionResult Register(AppUser model)
        {
            // Kiểm tra username đã tồn tại chưa
            if (_users.Any(x => x.Username == model.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
            }
            
            // Giả lập tạo user mới
            var newUser = new AppUser
            {
                Id = (_users.Count + 1).ToString(),
                Username = model.Username,
                Email = model.Email,
                Password = model.Password, // Trong thực tế, hash password trước khi lưu
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Role = "User" // Mặc định là User
            };
            
            _users.Add(newUser);
            
            var token = _jwtService.GenerateToken(newUser);
            
            return Ok(new AuthResponse
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName,
                Role = newUser.Role,
                Token = token
            });
        }
        
        // GET: api/Auth/profile
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var user = _users.SingleOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            
            // Không trả về password
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.Role
            });
        }
    }
}
