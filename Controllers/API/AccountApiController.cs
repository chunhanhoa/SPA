using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QL_Spa.Data;
using QL_Spa.Models;
using QL_Spa.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace QL_Spa.Controllers.Api
{
    [Route("api/Account")]  // Thay đổi route từ "api/[controller]" thành "api/Account"
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountApiController> _logger;
        private readonly SpaDbContext _context;
        private readonly JwtService _jwtService;

        public AccountApiController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountApiController> logger,
            SpaDbContext context,
            JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _jwtService = jwtService;
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
                _logger.LogInformation("Đã tạo tài khoản mới với mật khẩu.");

                // Gán vai trò User cho người dùng mới
                await _userManager.AddToRoleAsync(user, "User");

                // Tạo bản ghi Customer cho người dùng này
                var customer = new Customer
                {
                    UserId = user.Id,
                    FullName = model.Username,
                    Phone = "",
                    CreatedDate = DateTime.Now,
                    TotalAmount = 0
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Đăng nhập người dùng
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                // Tạo JWT token
                var token = await _jwtService.CreateTokenAsync(user);
                
                // Lưu token vào cookie
                SetJwtCookie(token);
                
                // Trả về token và thông tin người dùng
                return Ok(new { 
                    success = true,
                    message = "Đăng ký thành công", 
                    userId = user.Id,
                    username = user.UserName,
                    token = token 
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
                return BadRequest(new { 
                    success = false, 
                    message = "Thông tin đăng nhập không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // Kiểm tra xem người dùng tồn tại không
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Unauthorized(new { 
                    success = false, 
                    message = "Đăng nhập không thành công",
                    error = "Tên người dùng không tồn tại trong hệ thống."
                });
            }

            // Thử đăng nhập
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Người dùng đã đăng nhập: {Username}", model.Username);
                
                // Tạo JWT token
                var token = await _jwtService.CreateTokenAsync(user);
                
                // Lưu token vào cookie
                SetJwtCookie(token);
                
                // Lấy vai trò của người dùng
                var roles = await _userManager.GetRolesAsync(user);
                
                return Ok(new { 
                    success = true, 
                    message = "Đăng nhập thành công",
                    userId = user.Id,
                    username = user.UserName,
                    token = token,
                    roles = roles
                });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Tài khoản bị khóa: {Username}", model.Username);
                return StatusCode(423, new { 
                    success = false, 
                    message = "Tài khoản đã bị khóa",
                    error = "Vui lòng thử lại sau hoặc liên hệ quản trị viên."
                });
            }

            _logger.LogWarning("Đăng nhập thất bại cho người dùng: {Username}", model.Username);
            return Unauthorized(new { 
                success = false, 
                message = "Đăng nhập không thành công",
                error = "Mật khẩu không chính xác." 
            });
        }

        // POST: api/AccountApi/Logout
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Xóa token khỏi database
                    await _jwtService.RemoveTokenAsync(user);
                }
            }
            
            await _signInManager.SignOutAsync();
            
            // Xóa JWT token khỏi cookie
            Response.Cookies.Delete("jwtToken", new CookieOptions { 
                SameSite = SameSiteMode.Lax,
                Secure = true
            });
            
            _logger.LogInformation("Người dùng đã đăng xuất.");
            return Ok(new { success = true, message = "Đăng xuất thành công" });
        }
        
        // Helper method to set JWT cookie
        private void SetJwtCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Đảm bảo gửi qua HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(7) // Đồng bộ với thời gian hết hạn token
            };
            
            Response.Cookies.Append("jwtToken", token, cookieOptions);
        }
        
        // Kiểm tra token
        [HttpGet("ValidateToken")]
        public IActionResult ValidateToken()
        {
            // Nếu request đến đây và đã được xác thực, trả về success
            if (User.Identity.IsAuthenticated)
            {
                var roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();
                    
                return Ok(new {
                    success = true,
                    username = User.Identity.Name,
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    roles = roles
                });
            }
            
            return Unauthorized(new { 
                success = false, 
                message = "Token không hợp lệ hoặc đã hết hạn" 
            });
        }
    }
}
