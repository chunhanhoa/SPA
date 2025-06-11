using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System.Security.Claims;

namespace QL_Spa.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SpaDbContext _context;

        public AccountController(
            SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager,
            SpaDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username exists
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    ModelState.AddModelError("Username", "Tên người dùng không tồn tại trong hệ thống.");
                    return View(model);
                }

                // Try to sign in with the provided credentials
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                // Password is incorrect
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản bị khóa. Vui lòng thử lại sau.");
                }
                else
                {
                    ModelState.AddModelError("Password", "Mật khẩu không chính xác.");
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        ///them
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
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
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FullName = customer.FullName,
                Phone = customer.Phone
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            customer.FullName = model.FullName;
            customer.Phone = model.Phone;

            await _context.SaveChangesAsync();
            
            TempData["StatusMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";
            return RedirectToAction("EditProfile");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                // If somehow the customer record doesn't exist, create it
                customer = new Customer
                {
                    UserId = userId,
                    FullName = user.UserName,
                    Phone = "",
                    CreatedDate = DateTime.Now,
                    TotalAmount = 0
                };
                
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            var viewModel = new ProfileViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = customer.FullName,
                Phone = customer.Phone,
                CreatedDate = customer.CreatedDate,
                TotalAmount = customer.TotalAmount
            };

            return View(viewModel);
        }
    }
}