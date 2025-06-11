using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(
            UserManager<IdentityUser> userManager,
            JwtService jwtService,
            ILogger<TokenController> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        // GET: api/Token/Check
        [Authorize]
        [HttpGet("Check")]
        public async Task<IActionResult> CheckToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Không tìm thấy thông tin người dùng" });
            }

            var token = await _jwtService.GetTokenFromDatabaseAsync(userId);
            
            if (string.IsNullOrEmpty(token))
            {
                return NotFound(new { success = false, message = "Không tìm thấy token trong database" });
            }

            // Lấy tất cả token của user này từ database để kiểm tra
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy thông tin người dùng" });
            }
            
            var tokenExpiry = await _userManager.GetAuthenticationTokenAsync(user, "JwtProvider", "JwtExpiry");
            DateTime? expiryDate = null;
            
            if (!string.IsNullOrEmpty(tokenExpiry) && DateTime.TryParse(tokenExpiry, out var parsedDate))
            {
                expiryDate = parsedDate;
            }

            return Ok(new 
            { 
                success = true, 
                userId = userId,
                username = User.Identity.Name,
                tokenFound = !string.IsNullOrEmpty(token),
                tokenLength = token?.Length ?? 0,
                tokenPrefix = token?.Substring(0, Math.Min(20, token?.Length ?? 0)),
                tokenExpiry = expiryDate
            });
        }
        
        // GET: api/Token/ListAll
        [Authorize(Roles = "Admin")]
        [HttpGet("ListAll")]
        public async Task<IActionResult> ListAllTokens()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<object>();
            
            foreach (var user in users)
            {
                var token = await _userManager.GetAuthenticationTokenAsync(user, "JwtProvider", "JwtToken");
                var tokenExpiry = await _userManager.GetAuthenticationTokenAsync(user, "JwtProvider", "JwtExpiry");
                
                result.Add(new 
                {
                    userId = user.Id,
                    username = user.UserName,
                    hasToken = !string.IsNullOrEmpty(token),
                    tokenExpiry = !string.IsNullOrEmpty(tokenExpiry) ? tokenExpiry : null
                });
            }
            
            return Ok(result);
        }
    }
}
