using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QL_Spa.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class RevenueManagementController : Controller
    {
        private readonly ILogger<RevenueManagementController> _logger;

        public RevenueManagementController(ILogger<RevenueManagementController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Accessed Revenue Management page");
            return View("~/Views/Admin/RevenueManagement.cshtml");
        }
    }
}
