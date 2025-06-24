using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QL_Spa.Data;
using QL_Spa.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QL_Spa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SpaDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SpaDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserManagement()
        {
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

            return View(userViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                UserRoles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Remove roles that are not selected
            foreach (var role in userRoles)
            {
                if (!model.SelectedRoles.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }

            // Add newly selected roles
            foreach (var role in model.SelectedRoles)
            {
                if (!userRoles.Contains(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            return RedirectToAction("UserManagement");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is the current logged in admin
            if (User.Identity.Name == user.UserName)
            {
                TempData["ErrorMessage"] = "Bạn không thể xóa tài khoản hiện tại đang đăng nhập.";
                return RedirectToAction("UserManagement");
            }

            // Delete associated customer information if exists
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đã xóa người dùng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa người dùng.";
            }

            return RedirectToAction("UserManagement");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RoomManagement()
        {
            _logger.LogInformation("Admin accessing RoomManagement page");
            
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("Non-admin user attempted to access RoomManagement: {Username}", User.Identity.Name);
                return Forbid();
            }
            
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ChairManagement()
        {
            _logger.LogInformation("Admin accessing ChairManagement page");
            
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("Non-admin user attempted to access ChairManagement: {Username}", User.Identity.Name);
                return Forbid();
            }
            
            var rooms = _context.Rooms.ToList();
            ViewBag.Rooms = rooms;
            
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult BookingManagement()
        {
            _logger.LogInformation("Admin accessing BookingManagement page");
            
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("Non-admin user attempted to access BookingManagement: {Username}", User.Identity.Name);
                return Forbid();
            }
            
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult InvoiceManagement()
        {
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult ServiceManagement()
        {
            _logger.LogInformation("Admin accessing ServiceManagement page");
            
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("Non-admin user attempted to access ServiceManagement: {Username}", User.Identity.Name);
                return Forbid();
            }
            
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult RevenueManagement()
        {
            _logger.LogInformation("Admin accessing RevenueManagement page");
            
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("Non-admin user attempted to access RevenueManagement: {Username}", User.Identity.Name);
                return Forbid();
            }
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserRolesViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                Email = user.Email,
                Roles = new List<RoleViewModel>()
            };

            // Get all roles
            var roles = await _roleManager.Roles.ToListAsync();
            foreach (var role in roles)
            {
                var roleViewModel = new RoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                };
                model.Roles.Add(roleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user from existing roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, 
                model.Roles.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add user to selected roles");
                return View(model);
            }

            return RedirectToAction("EditUser", new { id = model.UserId });
        }
    }
}
