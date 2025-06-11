using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace QL_Spa.Controllers
{
    public class DebugController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public DebugController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(new UserInfoViewModel
                {
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    UserName = User.Identity.Name,
                    ErrorMessage = "User could not be found in the database"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return View(new UserInfoViewModel
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = user.UserName,
                Email = user.Email,
                UserId = user.Id,
                Roles = roles.ToList(),
                Claims = claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
            });
        }
    }

    public class UserInfoViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<ClaimInfo> Claims { get; set; } = new List<ClaimInfo>();
        public string ErrorMessage { get; set; }
    }

    public class ClaimInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
