using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/RoleApi")]
    [ApiController]
    [Authorize] // Require authentication to access this API
    public class RoleApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleApiController> _logger;

        public RoleApiController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RoleApiController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: api/RoleApi/UserRoles
        [HttpGet("UserRoles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        // GET: api/RoleApi/IsAdmin
        [HttpGet("IsAdmin")]
        public async Task<ActionResult<bool>> IsAdmin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            return Ok(new { isAdmin });
        }

        // GET: api/RoleApi/HasRole/{roleName}
        [HttpGet("HasRole/{roleName}")]
        public async Task<ActionResult<bool>> HasRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name cannot be empty");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var hasRole = await _userManager.IsInRoleAsync(user, roleName);
            return Ok(hasRole);
        }

        // GET: api/RoleApi/HasManagementAccess
        [HttpGet("HasManagementAccess")]
        public async Task<ActionResult<bool>> HasManagementAccess()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Now only Admin has management access
            var hasAccess = await _userManager.IsInRoleAsync(user, "Admin");
            
            return Ok(hasAccess);
        }
    }
}
