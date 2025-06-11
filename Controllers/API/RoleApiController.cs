using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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

        // GET: api/RoleApi
        [HttpGet]
        public ActionResult<IEnumerable<IdentityRole>> GetRoles()
        {
            return Ok(_roleManager.Roles.ToList());
        }

        // GET: api/RoleApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IdentityRole>> GetRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // POST: api/RoleApi
        [HttpPost]
        public async Task<ActionResult<IdentityRole>> CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name cannot be empty");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return Conflict($"Role '{roleName}' already exists");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetRole), new { id = roleName }, new IdentityRole(roleName));
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/RoleApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Check if users are assigned to this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                return BadRequest($"Cannot delete role '{role.Name}' because it has {usersInRole.Count} users assigned to it");
            }

            var result = await _roleManager.DeleteAsync(role);
            
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // GET: api/RoleApi/UsersInRole/Admin
        [HttpGet("UsersInRole/{roleName}")]
        public async Task<ActionResult<IEnumerable<string>>> GetUsersInRole(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound($"Role '{roleName}' not found");
            }

            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return Ok(users.Select(u => u.UserName).ToList());
        }
    }
}
