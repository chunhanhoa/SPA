using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_Spa.Data;

namespace QL_Spa.Controllers
{
    [Authorize(Roles = "Admin")] // Only users with Admin role can access
    public class ManagementController : Controller
    {
        private readonly SpaDbContext _context;

        public ManagementController(SpaDbContext context)
        {
            _context = context;
        }

        // GET: Management/Accounts
        public IActionResult Accounts()
        {
            ViewData["Title"] = "Quản lý Tài khoản";
            return View();
        }

        // GET: Management/Chairs
        public IActionResult Chairs()
        {
            ViewData["Title"] = "Quản lý Ghế";
            return View();
        }

        // GET: Management/Rooms
        public IActionResult Rooms()
        {
            ViewData["Title"] = "Quản lý Phòng";
            return View();
        }
    }
}
