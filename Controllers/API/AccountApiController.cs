using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QL_Spa.Data;
using QL_Spa.Models;
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
        private readonly SpaDbContext _context;

        public AccountApiController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountApiController> logger,
            SpaDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
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

                // Assign the User role to the new user
                await _userManager.AddToRoleAsync(user, "User");

                // Create a new Customer record for this user
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

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok(new { message = "User registered successfully", userId = user.Id });
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
                return Ok(new { message = "Login successful" });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return StatusCode(423, new { message = "Account locked out" });
            }

            return Unauthorized(new { message = "Invalid login attempt" });
        }

        // POST: api/AccountApi/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Ok(new { message = "Logout successful" });
        }
    }
}
