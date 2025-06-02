using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceApiController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<ServiceApiController> _logger;

        public ServiceApiController(SpaDbContext context, ILogger<ServiceApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServiceApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetServices()
        {
            try
            {
                _logger.LogInformation("Retrieving all services");

                var services = await _context.Services
                    .Select(s => new
                    {
                        s.ServiceId,
                        s.ServiceName,
                        s.Description,
                        s.Price,
                        s.Duration,
                        s.Picture,
                        s.Features,
                        s.Process,
                        s.Notes
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {services.Count} services");
                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ServiceApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving service with ID: {id}");

                var service = await _context.Services.FindAsync(id);

                if (service == null)
                {
                    _logger.LogWarning($"Service with ID {id} not found");
                    return NotFound($"Service with ID {id} not found");
                }

                // Ensure HTML is properly sanitized before returning it
                // In a production environment, consider using a HTML sanitizer library
                if (!string.IsNullOrEmpty(service.Features))
                {
                    service.Features = service.Features.Replace("<script>", "").Replace("</script>", "");
                }
                
                if (!string.IsNullOrEmpty(service.Process))
                {
                    service.Process = service.Process.Replace("<script>", "").Replace("</script>", "");
                }
                
                if (!string.IsNullOrEmpty(service.Notes))
                {
                    service.Notes = service.Notes.Replace("<script>", "").Replace("</script>", "");
                }

                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving service with ID {id}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Only admins can modify services
        // POST: api/ServiceApi
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            try
            {
                _logger.LogInformation("Creating new service: {ServiceName}", service.ServiceName);

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/ServiceApi/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, Service service)
        {
            if (id != service.ServiceId)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} doesn't match body ID {BodyId}", id, service.ServiceId);
                return BadRequest("ID mismatch between URL and body");
            }

            try
            {
                _logger.LogInformation("Updating service with ID: {ServiceId}", id);

                _context.Entry(service).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
                {
                    _logger.LogWarning($"Service with ID {id} not found during update");
                    return NotFound($"Service with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service with ID {ServiceId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ServiceApi/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                _logger.LogInformation("Deleting service with ID: {ServiceId}", id);

                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    _logger.LogWarning($"Service with ID {id} not found during deletion");
                    return NotFound($"Service with ID {id} not found");
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Service deleted successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service with ID {ServiceId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}
