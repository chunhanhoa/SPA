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
                    status = invoice.Status, // Include the status in the response
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

        // Lấy danh sách tất cả hóa đơn
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllInvoices()
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                var result = invoices.Select(invoice => new
                {
                    invoiceId = invoice.InvoiceId,
                    createdDate = invoice.CreatedDate,
                    totalAmount = invoice.TotalAmount,
                    discount = invoice.Discount,
                    finalAmount = invoice.FinalAmount,
                    paidAmount = invoice.PaidAmount,
                    status = invoice.Status,
                    customerName = invoice.Customer?.FullName ?? "N/A",
                    customerPhone = invoice.Customer?.Phone ?? "N/A"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách hóa đơn" });
            }
        }

        // Cập nhật trạng thái hóa đơn
        [HttpPost("{id}/UpdateStatus")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] UpdateInvoiceStatusRequest request)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Validate status
                var validStatuses = new[] { "Chờ thanh toán", "Đã thanh toán", "Đã hủy", "Hoàn tiền" };
                if (!validStatuses.Contains(request.Status))
                {
                    return BadRequest(new { success = false, message = "Trạng thái không hợp lệ" });
                }

                invoice.Status = request.Status;
                
                // If status is "Đã thanh toán", update PaidAmount to FinalAmount
                if (request.Status == "Đã thanh toán" && invoice.PaidAmount < invoice.FinalAmount)
                {
                    invoice.PaidAmount = invoice.FinalAmount;
                }

                await _context.SaveChangesAsync();
                
                return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái hóa đơn ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật trạng thái hóa đơn" });
            }
        }

        // Tạo hóa đơn mới từ lịch hẹn
        [HttpPost("CreateFromAppointment/{appointmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateInvoiceFromAppointment(int appointmentId)
        {
            try
            {
                _logger.LogInformation("Bắt đầu tạo hóa đơn từ lịch hẹn ID: {ID}", appointmentId);
                
                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning("Không tìm thấy lịch hẹn ID: {ID}", appointmentId);
                    return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });
                }

                // Check if invoice already exists for this appointment
                var existingInvoice = await _context.Invoices
                    .Where(i => i.CustomerId == appointment.CustomerId)
                    .Where(i => i.CreatedDate.Date == appointment.CreatedDate.Date)
                    .Where(i => Math.Abs(i.TotalAmount - appointment.TotalAmount) < 0.01m)
                    .FirstOrDefaultAsync();

                if (existingInvoice != null)
                {
                    _logger.LogWarning("Hóa đơn tương tự đã tồn tại cho lịch hẹn ID: {ID}, InvoiceId: {InvoiceId}", 
                        appointmentId, existingInvoice.InvoiceId);
                    return BadRequest(new { success = false, message = "Hóa đơn đã tồn tại cho lịch hẹn này" });
                }

                // Create new invoice with all required fields
                var invoice = new Invoice
                {
                    CreatedDate = DateTime.Now,
                    TotalAmount = appointment.TotalAmount,
                    Discount = 0, // Default discount
                    PaidAmount = 0, // Default paid amount
                    CustomerId = appointment.CustomerId,
                    Status = "Chờ thanh toán",
                    InvoiceServices = new List<InvoiceService>()
                };

                _logger.LogInformation("Tạo hóa đơn mới: CustomerId={CustomerId}, TotalAmount={TotalAmount}", 
                    invoice.CustomerId, invoice.TotalAmount);

                // Add services from appointment
                foreach (var appService in appointment.AppointmentServices)
                {
                    var invoiceService = new InvoiceService
                    {
                        ServiceId = appService.ServiceId,
                        Quantity = appService.Quantity,
                        Price = appService.Price
                    };
                    
                    _logger.LogInformation("Thêm dịch vụ: ServiceId={ServiceId}, Quantity={Quantity}, Price={Price}", 
                        invoiceService.ServiceId, invoiceService.Quantity, invoiceService.Price);
                
                    invoice.InvoiceServices.Add(invoiceService);
                }

                // Use explicit transaction for consistency
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Invoices.Add(invoice);
                        await _context.SaveChangesAsync();
                        
                        // Update appointment status if needed
                        if (appointment.Status != "Hoàn thành")
                        {
                            appointment.Status = "Hoàn thành";
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Đã cập nhật trạng thái lịch hẹn thành 'Hoàn thành'");
                        }
                        
                        await transaction.CommitAsync();
                        _logger.LogInformation("Giao dịch đã được commit thành công");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Lỗi trong giao dịch, đã rollback: {Message}", ex.Message);
                        throw; // Re-throw to be caught by outer catch
                    }
                }

                _logger.LogInformation("Đã tạo hóa đơn thành công: InvoiceId={InvoiceId}", invoice.InvoiceId);
                return Ok(new { 
                    success = true, 
                    message = "Tạo hóa đơn thành công", 
                    invoiceId = invoice.InvoiceId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hóa đơn từ lịch hẹn ID: {ID}: {Message}", appointmentId, ex.Message);
                return StatusCode(500, new { success = false, message = $"Lỗi khi tạo hóa đơn: {ex.Message}" });
            }
        }

        // Cập nhật thông tin thanh toán
        [HttpPost("{id}/UpdatePayment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentRequest request)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Update payment information
                if (request.Discount.HasValue)
                {
                    invoice.Discount = request.Discount.Value;
                }

                if (request.PaidAmount.HasValue)
                {
                    invoice.PaidAmount = request.PaidAmount.Value;
                    
                    // Update status based on paid amount
                    if (invoice.PaidAmount >= invoice.FinalAmount)
                    {
                        invoice.Status = "Đã thanh toán";
                    }
                    else if (invoice.PaidAmount > 0)
                    {
                        invoice.Status = "Chờ thanh toán";
                    }
                }

                await _context.SaveChangesAsync();
                
                return Ok(new { 
                    success = true, 
                    message = "Cập nhật thông tin thanh toán thành công",
                    finalAmount = invoice.FinalAmount,
                    paidAmount = invoice.PaidAmount,
                    status = invoice.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin thanh toán cho hóa đơn ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật thông tin thanh toán" });
            }
        }
    }

    public class UpdateInvoiceStatusRequest
    {
        [Required]
        public string Status { get; set; }
    }

    public class UpdatePaymentRequest
    {
        public decimal? Discount { get; set; }
        public decimal? PaidAmount { get; set; }
    }
}