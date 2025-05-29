using LoanSpa.Data;
using LoanSpa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(SpaDbContext context, ILogger<ServicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            try
            {
                _logger.LogInformation("Đang truy vấn tất cả dịch vụ");
                var services = await _context.Services.ToListAsync();
                _logger.LogInformation($"Đã truy xuất {services.Count} dịch vụ");
                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn dịch vụ");
                return StatusCode(500, new { message = "Lỗi server khi truy xuất dữ liệu dịch vụ" });
            }
        }

        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            try
            {
                _logger.LogInformation($"Đang truy vấn dịch vụ ID={id}");
                var service = await _context.Services.FindAsync(id);

                if (service == null)
                {
                    _logger.LogWarning($"Không tìm thấy dịch vụ ID={id}");
                    return NotFound();
                }

                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi truy vấn dịch vụ ID={id}");
                return StatusCode(500, new { message = "Lỗi server khi truy xuất dữ liệu dịch vụ" });
            }
        }
    }
}
