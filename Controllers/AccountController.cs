using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LoanSpa.Models;
using System.Threading.Tasks;
using LoanSpa.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http; // Thêm namespace này

namespace LoanSpa.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly SpaDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            SpaDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
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
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Người dùng đã đăng nhập thành công.");
                    
                    // Lấy user và customer
                    var user = await _userManager.FindByNameAsync(model.Username);
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);
                    
                    // Lưu tên người dùng vào session
                    if (customer != null)
                    {
                        HttpContext.Session.SetString("CustomerFullName", customer.FullName);
                    }
                    
                    TempData["SuccessMessage"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Tài khoản đã bị khóa.");
                    return RedirectToAction("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập thất bại, vui lòng kiểm tra lại thông tin.");
                    return View(model);
                }
            }
            return View(model);
        }

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
                var user = new IdentityUser { UserName = model.Username, Email = model.Email, PhoneNumber = model.PhoneNumber };
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Người dùng đã tạo tài khoản mới với mật khẩu.");
                    
                    // Kiểm tra và tạo vai trò "User" nếu chưa tồn tại
                    var roleExists = await _roleManager.RoleExistsAsync("User");
                    if (!roleExists)
                    {
                        var role = new IdentityRole("User");
                        await _roleManager.CreateAsync(role);
                    }
                    
                    // Gán vai trò "User" cho người dùng mới
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    // Tạo Customer mới và lưu vào database
                    var customer = new Customer
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        Phone = model.PhoneNumber,
                        CreatedDate = DateTime.Now,
                        TotalAmount = 0
                    };
                    
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
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

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Xóa thông tin Session khi đăng xuất
            HttpContext.Session.Remove("CustomerFullName");
            
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Người dùng đã đăng xuất.");
            return RedirectToAction("Index", "Home");
        }
    }
}