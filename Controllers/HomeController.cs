using System.Diagnostics;
using LoanSpa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanSpa.Data;


namespace LoanSpa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy 6 dịch vụ nổi bật để hiển thị trên trang chủ
            var featuredServices = await _context.Services
                .OrderBy(s => s.ServiceId)
                .Take(6)
                .ToListAsync();
            
            return View(featuredServices);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Services()
        {
            // Lấy tất cả dịch vụ từ database
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        public IActionResult Booking()
        {
            return View();
        }

        public IActionResult Gallery()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public async Task<IActionResult> ServiceDetail(int id)
        {
            // Lấy thông tin chi tiết dịch vụ từ database theo id
            var service = await _context.Services.FindAsync(id);
            
            // Nếu không tìm thấy dịch vụ, trả về trang lỗi hoặc chuyển hướng
            if (service == null)
            {
                return RedirectToAction("Services");
            }
            
            return View(service);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
