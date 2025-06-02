using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileApiController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProfileApiController> _logger;

        public ProfileApiController(
            SpaDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<ProfileApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<ActionResult<Customer>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(Customer customer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Only update specific fields to prevent overriding sensitive data
            existingCustomer.FullName = customer.FullName;
            existingCustomer.Phone = customer.Phone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(existingCustomer.CustomerId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
