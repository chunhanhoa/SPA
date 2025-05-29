using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanSpa.Data;
using System.Threading.Tasks;

namespace QL_Spa.Controllers
{
    public class RoomController : Controller
    {
        private readonly SpaDbContext _context;

        public RoomController(SpaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
