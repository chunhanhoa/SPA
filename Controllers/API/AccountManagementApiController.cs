using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/AccountManagement")] // Changed from [Route("api/[controller]")] to match the frontend call
    [ApiController]
    [Authorize] // Only authenticated users can access this API
    public class AccountManagementApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SpaDbContext _context;
        private readonly ILogger<AccountManagementApiController> _logger;

        public AccountManagementApiController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SpaDbContext context,
            ILogger<AccountManagementApiController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: api/AccountManagement
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetAccounts()
        {
            try
            {
                _logger.LogInformation("Fetching all user accounts");
                
                var users = await _userManager.Users.ToListAsync();
                var userViewModels = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = roles.ToList(),
                        FullName = customer?.FullName,
                        CreatedDate = customer?.CreatedDate
                    });
                }

                _logger.LogInformation($"Retrieved {userViewModels.Count} user accounts");
                return userViewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user accounts");
                return StatusCode(500, "An error occurred while retrieving user accounts");
            }
        }

        // GET: api/AccountManagement/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserViewModel>> GetAccount(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == id);

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                FullName = customer?.FullName,
                CreatedDate = customer?.CreatedDate
            };

            return userViewModel;
        }
    }
}
