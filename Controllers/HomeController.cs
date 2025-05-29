using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LoanSpa.Models;
using LoanSpa.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LoanSpa.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SpaDbContext _context;
    private readonly UserManager<IdentityUser> _userManager; // Sửa thành IdentityUser

    public HomeController(ILogger<HomeController> logger, SpaDbContext context, UserManager<IdentityUser> userManager) // Sửa thành IdentityUser
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    private async Task SetCustomerInfo()
    {
        if (User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);
                
            if (customer != null)
            {
                ViewBag.CustomerFullName = customer.FullName;
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.CustomerFullName = user?.UserName;
            }
        }
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            await SetCustomerInfo();
            
            // Lấy 6 dịch vụ nổi bật để hiển thị trên trang chủ
            var featuredServices = await _context.Services
                .OrderBy(s => s.ServiceId)
                .Take(6)
                .ToListAsync();
            
            _logger.LogInformation($"Found {featuredServices.Count} featured services for home page");
            
            return View(featuredServices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải dữ liệu dịch vụ cho trang chủ");
            return View(new List<Service>());
        }
    }

    public async Task<IActionResult> Privacy()
    {
        await SetCustomerInfo();
        return View();
    }

    public async Task<IActionResult> About()
    {
        await SetCustomerInfo();
        return View();
    }

    public async Task<IActionResult> Contact()
    {
        await SetCustomerInfo();
        return View();
    }

    public async Task<IActionResult> Booking()
    {
        await SetCustomerInfo();
        return View();
    }

    // Thêm log để debug
    public async Task<IActionResult> Services()
    {
        await SetCustomerInfo();
        
        try
        {
            // Đếm số lượng dịch vụ
            var count = await _context.Services.CountAsync();
            _logger.LogInformation($"Found {count} services in database");
            
            // Lấy tất cả dịch vụ từ database
            var services = await _context.Services.ToListAsync();
            return View(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading services");
            return View(new List<Service>());
        }
    }

    public async Task<IActionResult> ServiceDetail(int id)
    {
        await SetCustomerInfo();
        
        try
        {
            // Lấy thông tin chi tiết dịch vụ từ database theo id
            var service = await _context.Services.FindAsync(id);
            _logger.LogInformation($"Loading service detail for ID {id}: {service?.ServiceName ?? "Not found"}");
            
            // Nếu không tìm thấy dịch vụ, trả về trang lỗi hoặc chuyển hướng
            if (service == null)
            {
                return RedirectToAction("Services");
            }
            
            return View(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading service detail for ID {id}");
            return RedirectToAction("Services");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
