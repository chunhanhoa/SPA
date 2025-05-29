using LoanSpa.Data;
using LoanSpa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly SpaDbContext _context;

        public TestController(SpaDbContext context)
        {
            _context = context;
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.Services.ToListAsync();
            return Ok(new { 
                count = services.Count,
                data = services
            });
        }
    }
}
