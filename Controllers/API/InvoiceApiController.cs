using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace QL_Spa.Controllers.Api
{
    [ApiController]
    [Route("api/Invoice")]
    public class InvoiceApiController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<InvoiceApiController> _logger;

        public InvoiceApiController(SpaDbContext context, ILogger<InvoiceApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy chi tiết hóa đơn
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceServices)
                        .ThenInclude(invService => invService.Service)
                    .FirstOrDefaultAsync(i => i.InvoiceId == id);

                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                var result = new
                {
                    invoiceId = invoice.InvoiceId,
                    createdDate = invoice.CreatedDate,
                    totalAmount = invoice.TotalAmount,
                    discount = invoice.Discount,
                    finalAmount = invoice.FinalAmount,
                    paidAmount = invoice.PaidAmount,
                    customer = invoice.Customer != null ? new
                    {
                        fullName = invoice.Customer.FullName,
                        phone = invoice.Customer.Phone
                    } : null,
                    services = invoice.InvoiceServices.Select(s => new
                    {
                        serviceId = s.ServiceId,
                        serviceName = s.Service.ServiceName,
                        price = s.Price,
                        quantity = s.Quantity
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết hóa đơn ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy chi tiết hóa đơn" });
            }
        }
    }
}