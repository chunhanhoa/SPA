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
using System.IO; // Thêm namespace này
using iTextSharp.text; // Thêm namespace cho thư viện iTextSharp
using iTextSharp.text.pdf; // Thêm namespace cho thư viện iTextSharp

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

                string oldStatus = invoice.Status;
                invoice.Status = request.Status;
                
                // If status is "Đã thanh toán", update PaidAmount to FinalAmount
                if (request.Status == "Đã thanh toán" && invoice.PaidAmount < invoice.FinalAmount)
                {
                    invoice.PaidAmount = invoice.FinalAmount;
                }

                await _context.SaveChangesAsync();

                // Nếu trạng thái được chuyển sang "Đã thanh toán", chỉ cập nhật trạng thái của các ghế liên quan đến hóa đơn này
                if (request.Status == "Đã thanh toán" && oldStatus != "Đã thanh toán")
                {
                    _logger.LogInformation($"Cập nhật trạng thái ghế cho hóa đơn ID: {id}");

                    try {
                        // Lỗi khi cố gắng truy cập bảng InvoiceDetails và sử dụng .HasValue và .Value trên kiểu int
                        try {
                            // Kiểm tra xem DbSet InvoiceDetails có tồn tại không
                            bool invoiceDetailsExist = false;
                            try {
                                invoiceDetailsExist = _context.Model.FindEntityType(typeof(InvoiceDetail)) != null;
                            } catch {
                                invoiceDetailsExist = false;
                            }

                            // Nếu bảng InvoiceDetail tồn tại, sử dụng nó để tìm ghế
                            if (invoiceDetailsExist)
                            {
                                try {
                                    // Sửa: Bỏ qua đoạn code này vì bảng InvoiceDetail không tồn tại
                                    // Thay vì cố gắng truy vấn bảng không tồn tại, chúng ta sẽ sử dụng Appointment
                                } catch (Exception ex) {
                                    _logger.LogError($"Lỗi khi truy vấn InvoiceDetail: {ex.Message}");
                                }
                            }

                            // Sửa phương thức tìm lịch hẹn trong khối try của UpdateInvoiceStatus
                            // Tìm lịch hẹn cụ thể liên quan đến hóa đơn này (dựa trên CustomerId và CreatedDate)
                            var relatedAppointment = await _context.Appointments
                                .Include(a => a.AppointmentChairs)
                                .Where(a => a.CustomerId == invoice.CustomerId)
                                .Where(a => a.CreatedDate.Date == invoice.CreatedDate.Date)
                                // Thêm điều kiện để liên kết chính xác với hóa đơn - dựa trên TotalAmount
                                .Where(a => Math.Abs(a.TotalAmount - invoice.TotalAmount) < 0.01m)
                                .OrderByDescending(a => a.CreatedDate) // Thêm sắp xếp theo thời gian tạo mới nhất
                                .FirstOrDefaultAsync();

                            if (relatedAppointment != null)
                            {
                                _logger.LogInformation($"Tìm thấy lịch hẹn ID: {relatedAppointment.AppointmentId} liên quan đến hóa đơn");

                                // Cập nhật trực tiếp tất cả ghế liên quan
                                foreach (var appointmentChair in relatedAppointment.AppointmentChairs)
                                {
                                    var chair = await _context.Chairs.FindAsync(appointmentChair.ChairId);
                                    if (chair != null)
                                    {
                                        _logger.LogInformation($"Cập nhật ghế ID: {chair.ChairId}, Tên: {chair.ChairName} từ IsAvailable={chair.IsAvailable} thành IsAvailable=true");
                                        chair.IsAvailable = true;
                                        _context.Entry(chair).State = EntityState.Modified;
                                    }
                                }
                            }
                            else
                            {
                                // Nếu không tìm thấy lịch hẹn liên quan, thử tìm và cập nhật tất cả ghế có IsAvailable = false
                                _logger.LogInformation("Không tìm thấy lịch hẹn liên quan, tiến hành quét tất cả ghế đang bị khóa");
                                var lockedChairs = await _context.Chairs.Where(c => !c.IsAvailable).Take(5).ToListAsync();
                                foreach (var chair in lockedChairs)
                                {
                                    _logger.LogInformation($"Cập nhật ghế bị khóa ID: {chair.ChairId}, Tên: {chair.ChairName} thành IsAvailable=true");
                                    chair.IsAvailable = true;
                                    _context.Entry(chair).State = EntityState.Modified;
                                }
                            }

                            // Lưu những thay đổi của ghế vào cơ sở dữ liệu
                            var changes = await _context.SaveChangesAsync();
                            _logger.LogInformation($"Đã lưu {changes} thay đổi vào cơ sở dữ liệu");
                        }
                        catch (Exception ex) {
                            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái ghế: {Message}", ex.Message);
                        }
                    }
                    catch (Exception ex) {
                            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái ghế: {Message}", ex.Message);
                        }
                }

                return Ok(new { success = true, message = "Cập nhật trạng thái hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái hóa đơn ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật trạng thái hóa đơn" });
            }
        }

        // Xuất hóa đơn ra file PDF
        [HttpGet("{id}/ExportToPdf")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportInvoiceToPdf(int id)
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

                // Tạo file PDF từ hóa đơn (sử dụng thư viện iTextSharp hoặc tương tự)
                using (var memoryStream = new MemoryStream())
                {
                    // Khởi tạo document với cú pháp đúng
                    var document = new Document();
                    PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Thêm nội dung vào PDF
                    document.Add(new Paragraph($"Hóa đơn ID: {invoice.InvoiceId}"));
                    document.Add(new Paragraph($"Khách hàng: {invoice.Customer?.FullName}"));
                    document.Add(new Paragraph($"Số điện thoại: {invoice.Customer?.Phone}"));
                    document.Add(new Paragraph($"Ngày tạo: {invoice.CreatedDate}"));
                    document.Add(new Paragraph($"Tổng tiền: {invoice.TotalAmount}"));
                    document.Add(new Paragraph($"Giảm giá: {invoice.Discount}"));
                    document.Add(new Paragraph($"Thành tiền: {invoice.FinalAmount}"));
                    document.Add(new Paragraph($"Đã thanh toán: {invoice.PaidAmount}"));
                    document.Add(new Paragraph($"Trạng thái: {invoice.Status}"));

                    // Thêm bảng dịch vụ
                    var table = new PdfPTable(4);
                    table.AddCell("Dịch vụ");
                    table.AddCell("Giá");
                    table.AddCell("Số lượng");
                    table.AddCell("Thành tiền");

                    foreach (var invService in invoice.InvoiceServices)
                    {
                        table.AddCell(invService.Service.ServiceName);
                        table.AddCell(invService.Price.ToString());
                        table.AddCell(invService.Quantity.ToString());
                        table.AddCell((invService.Price * invService.Quantity).ToString()); // Tính TotalAmount = Price * Quantity
                    }

                    document.Add(table);

                    document.Close();
                    
                    // Lưu file PDF vào đĩa hoặc trả về cho người dùng tải về
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/invoices", $"invoice_{invoice.InvoiceId}.pdf");
                    System.IO.File.WriteAllBytes(filePath, memoryStream.ToArray());

                    return Ok(new { success = true, message = "Xuất hóa đơn thành công", filePath });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất hóa đơn ra PDF ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi xuất hóa đơn ra PDF" });
            }
        }

        // Cập nhật trạng thái hóa đơn - phiên bản đơn giản
        [HttpPost("UpdateInvoiceStatus")]
        public IActionResult UpdateInvoiceStatus(int invoiceId, string status)
        {
            try
            {
                // Lấy thông tin hóa đơn từ ID
                var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    return NotFound("Không tìm thấy hóa đơn");
                }

                // Cập nhật trạng thái hóa đơn
                invoice.Status = status;
                
                // Nếu trạng thái là "Đã thanh toán", nhả ghế (đặt IsAvailable = true)
                if (status == "Đã thanh toán")
                {
                    // Tìm lịch hẹn liên quan để giải phóng ghế
                    var relatedAppointment = _context.Appointments
                        .Include(a => a.AppointmentChairs)
                        .Where(a => a.CustomerId == invoice.CustomerId)
                        .Where(a => a.CreatedDate.Date == invoice.CreatedDate.Date)
                        .OrderByDescending(a => a.CreatedDate) // Thêm sắp xếp theo thời gian tạo mới nhất
                        .FirstOrDefault();
                    
                    if (relatedAppointment != null)
                    {
                        foreach (var appChair in relatedAppointment.AppointmentChairs)
                        {
                            var chair = _context.Chairs.FirstOrDefault(c => c.ChairId == appChair.ChairId);
                            if (chair != null)
                            {
                                chair.IsAvailable = true;
                            }
                        }
                    }
                }

                // Lưu các thay đổi vào database
                _context.SaveChanges();

                return Ok(new { success = true, message = "Cập nhật trạng thái hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // New endpoint to update payment information
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

                // Validate input
                if (request.TotalAmount <= 0)
                {
                    return BadRequest(new { success = false, message = "Tổng tiền phải lớn hơn 0" });
                }

                if (request.Discount < 0 || request.Discount > 100)
                {
                    return BadRequest(new { success = false, message = "Giảm giá phải từ 0% đến 100%" });
                }

                if (request.PaidAmount < 0)
                {
                    return BadRequest(new { success = false, message = "Số tiền đã thanh toán không thể âm" });
                }

                // Update invoice payment information
                invoice.TotalAmount = request.TotalAmount;
                invoice.Discount = request.Discount;
                invoice.PaidAmount = request.PaidAmount;

                // Calculate the FinalAmount (done automatically by computed column in database)
                // If paid in full, update status to "Đã thanh toán"
                decimal finalAmount = invoice.TotalAmount - (invoice.TotalAmount * invoice.Discount / 100);
                if (invoice.PaidAmount >= finalAmount && invoice.Status == "Chờ thanh toán")
                {
                    invoice.Status = "Đã thanh toán";
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Cập nhật thông tin thanh toán thành công",
                    data = new {
                        invoiceId = invoice.InvoiceId,
                        totalAmount = invoice.TotalAmount,
                        discount = invoice.Discount,
                        finalAmount = finalAmount,
                        paidAmount = invoice.PaidAmount,
                        status = invoice.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin thanh toán ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật thông tin thanh toán" });
            }
        }

        // GET: api/Invoice/Statistics
        [HttpGet("Statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInvoiceStatistics()
        {
            try
            {
                var total = await _context.Invoices.CountAsync();
                var pending = await _context.Invoices.CountAsync(i => i.Status == "Chờ thanh toán");
                var paid = await _context.Invoices.CountAsync(i => i.Status == "Đã thanh toán");
                var cancelled = await _context.Invoices.CountAsync(i => i.Status == "Đã hủy");
                
                return Ok(new {
                    total,
                    pending,
                    paid,
                    cancelled
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê hóa đơn");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê hóa đơn" });
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
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public decimal Discount { get; set; }
        
        [Required]
        public decimal PaidAmount { get; set; }
    }
}