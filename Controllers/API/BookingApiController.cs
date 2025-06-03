using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace QL_Spa.Controllers.Api
{
    [ApiController]
    [Route("api/Booking")]
    public class BookingApiController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BookingApiController> _logger;

        public BookingApiController(
            SpaDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<BookingApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // Lấy tất cả lịch hẹn (giữ nguyên)
        [Authorize(Roles = "Admin")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                _logger.LogInformation("Đang lấy danh sách tất cả lịch hẹn");
                var appointments = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .Include(a => a.AppointmentChairs)
                        .ThenInclude(apc => apc.Chair)
                            .ThenInclude(c => c.Room)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                var result = appointments.Select(appointment => new
                {
                    appointmentId = appointment.AppointmentId,
                    customer = appointment.Customer != null ? new
                    {
                        fullName = appointment.Customer.FullName,
                        phone = appointment.Customer.Phone
                    } : null,
                    startTime = appointment.StartTime,
                    endTime = appointment.EndTime,
                    status = appointment.Status ?? "Chờ xác nhận",
                    totalAmount = appointment.TotalAmount,
                    services = GetServicesList(appointment.AppointmentServices),
                    rooms = GetRoomsList(appointment.AppointmentChairs)
                }).ToList();

                _logger.LogInformation("Đã lấy danh sách tất cả lịch hẹn thành công, số lượng: {Count}", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tất cả lịch hẹn");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách lịch hẹn" });
            }
        }

        // Cập nhật trạng thái lịch hẹn (giữ nguyên)
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                _logger.LogInformation("Đang cập nhật trạng thái cho lịch hẹn ID: {ID}", id);
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                if (appointment == null)
                {
                    _logger.LogWarning("Không tìm thấy lịch hẹn ID: {ID}", id);
                    return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });
                }

                var validStatuses = new[] { "Chờ xác nhận", "Đã xác nhận", "Đang thực hiện", "Hoàn thành", "Đã hủy" };
                if (!validStatuses.Contains(request.Status))
                {
                    _logger.LogWarning("Trạng thái không hợp lệ: {Status}", request.Status);
                    return BadRequest(new { success = false, message = "Trạng thái không hợp lệ" });
                }

                appointment.Status = request.Status;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật trạng thái lịch hẹn ID: {ID} thành {Status}", id, request.Status);
                return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái lịch hẹn ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái" });
            }
        }

        // Lấy chi tiết lịch hẹn (giữ nguyên)
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            try
            {
                _logger.LogInformation("Đang lấy chi tiết đặt lịch ID: {ID}", id);
                var userId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
                var isAdmin = User.IsInRole("Admin");

                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .Include(a => a.AppointmentChairs)
                        .ThenInclude(apc => apc.Chair)
                            .ThenInclude(c => c.Room)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                if (appointment == null)
                {
                    _logger.LogWarning("Không tìm thấy đặt lịch ID: {ID}", id);
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin đặt lịch" });
                }

                if (!isAdmin && User.Identity.IsAuthenticated)
                {
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized();
                    }

                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null || customer.CustomerId != appointment.CustomerId)
                    {
                        _logger.LogWarning("Người dùng không có quyền xem lịch hẹn ID: {ID}", id);
                        return Forbid();
                    }
                }

                var result = new
                {
                    appointment = new
                    {
                        AppointmentId = appointment.AppointmentId,
                        CreatedDate = appointment.CreatedDate,
                        StartTime = appointment.StartTime,
                        EndTime = appointment.EndTime,
                        Status = appointment.Status ?? "Chờ xác nhận",
                        TotalAmount = appointment.TotalAmount,
                        Notes = appointment.Notes,
                        Customer = appointment.Customer != null ? new
                        {
                            CustomerId = appointment.Customer.CustomerId,
                            FullName = appointment.Customer.FullName,
                            Phone = appointment.Customer.Phone
                        } : null
                    },
                    services = GetServicesList(appointment.AppointmentServices),
                    chairs = GetChairsList(appointment.AppointmentChairs),
                    rooms = GetRoomsList(appointment.AppointmentChairs)
                };

                _logger.LogInformation("Đã lấy thông tin chi tiết đặt lịch ID: {ID} thành công", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết đặt lịch ID: {ID}", id);
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin chi tiết đặt lịch" });
            }
        }

        // Lấy lịch hẹn của người dùng (giữ nguyên)
        [Authorize]
        [HttpGet("MyBookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                _logger.LogInformation("Đang lấy danh sách lịch đã đặt của người dùng");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Không tìm thấy thông tin người dùng");
                    return Unauthorized(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                {
                    _logger.LogWarning("Không tìm thấy khách hàng liên kết với UserId: {UserId}", userId);
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
                }

                var appointments = await _context.Appointments
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .Include(a => a.AppointmentChairs)
                        .ThenInclude(apc => apc.Chair)
                            .ThenInclude(c => c.Room)
                    .Where(a => a.CustomerId == customer.CustomerId)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                var result = appointments.Select(appointment => new
                {
                    appointmentId = appointment.AppointmentId,
                    createdDate = appointment.CreatedDate,
                    startTime = appointment.StartTime,
                    endTime = appointment.EndTime,
                    status = appointment.Status ?? "Chờ xác nhận",
                    totalAmount = appointment.TotalAmount,
                    notes = appointment.Notes,
                    services = GetServicesList(appointment.AppointmentServices),
                    chairs = GetChairsList(appointment.AppointmentChairs),
                    rooms = GetRoomsList(appointment.AppointmentChairs)
                }).ToList();

                _logger.LogInformation("Đã lấy danh sách lịch đã đặt thành công cho khách hàng ID: {CustomerId}", customer.CustomerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch đã đặt");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách lịch đã đặt" });
            }
        }

        // Tạo lịch hẹn mới
        // POST: api/Booking/Create
        [HttpPost("Create")]
        [AllowAnonymous] // Tạm thời cho phép đặt lịch không cần đăng nhập
        public async Task<IActionResult> CreateBooking([FromBody] BookingCreateModel model)
        {
            try
            {
                _logger.LogInformation("Đang xử lý yêu cầu đặt lịch mới: {@Model}", model);

                if (model == null || model.StartTime == default || model.Services == null || !model.Services.Any())
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ hoặc chưa chọn dịch vụ" });
                }

                // Lấy ID người dùng hiện tại nếu đã đăng nhập
                var userId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
                Customer customer = null;

                // Nếu người dùng đăng nhập, lấy thông tin khách hàng
                if (!string.IsNullOrEmpty(userId))
                {
                    customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                }

                // Nếu không tìm thấy khách hàng, sử dụng khách hàng mặc định (ID = 1)
                if (customer == null)
                {
                    customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == 1);
                    
                    // Nếu không có khách hàng mặc định, tạo mới
                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            FullName = "Khách vãng lai",
                            Phone = "0000000000",
                            CreatedDate = DateTime.Now,
                            TotalAmount = 0
                        };
                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();
                    }
                }

                // Validate dịch vụ và tính tổng số lượng, thời gian
                var serviceIds = model.Services.Select(s => s.ServiceId).ToList();
                var services = await _context.Services
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .ToListAsync();

                if (services.Count != serviceIds.Count)
                {
                    return BadRequest(new { success = false, message = "Một hoặc nhiều dịch vụ không tồn tại" });
                }

                var totalQuantity = model.Services.Sum(s => s.Quantity);
                var totalAmount = 0m;

                foreach (var reqService in model.Services)
                {
                    var service = services.First(s => s.ServiceId == reqService.ServiceId);
                    totalAmount += service.Price * reqService.Quantity;
                }

                // Tạo lịch hẹn
                var appointment = new Appointment
                {
                    CustomerId = customer.CustomerId,
                    CreatedDate = DateTime.Now,
                    StartTime = model.StartTime,
                    EndTime = model.StartTime.AddMinutes(model.Duration),
                    TotalAmount = totalAmount,
                    Status = "Chờ xác nhận",
                    Notes = model.Notes ?? "",
                    AppointmentServices = new List<AppointmentService>(),
                    AppointmentChairs = new List<AppointmentChair>()
                };

                // Thêm dịch vụ
                foreach (var reqService in model.Services)
                {
                    var service = services.First(s => s.ServiceId == reqService.ServiceId);
                    appointment.AppointmentServices.Add(new AppointmentService
                    {
                        ServiceId = service.ServiceId,
                        Price = service.Price,
                        Quantity = reqService.Quantity
                    });
                }

                // Tìm ghế trống và gán cho lịch hẹn
                var availableChairs = await _context.Chairs
                    .Where(c => c.IsAvailable)
                    .Take(Math.Max(1, totalQuantity)) // Đảm bảo ít nhất 1 ghế
                    .ToListAsync();

                if (availableChairs.Count < totalQuantity)
                {
                    // Nếu không đủ ghế, vẫn cho đặt lịch nhưng ghi chú lại
                    appointment.Notes += " (Thiếu ghế, cần liên hệ khách hàng)";
                }

                // Gán ghế và cập nhật trạng thái
                foreach (var chair in availableChairs)
                {
                    appointment.AppointmentChairs.Add(new AppointmentChair
                    {
                        ChairId = chair.ChairId
                    });
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo lịch hẹn mới với ID: {ID}", appointment.AppointmentId);
                
                return Ok(new { 
                    message = "Đặt lịch thành công", 
                    appointmentId = appointment.AppointmentId,
                    redirectUrl = $"/Home/BookingConfirmation/{appointment.AppointmentId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch hẹn mới: {Error}", ex.Message);
                return StatusCode(500, new { 
                    success = false, 
                    message = "Đã xảy ra lỗi khi tạo lịch hẹn",
                    error = ex.Message
                });
            }
        }

        // Kiểm tra ghế có sẵn
        [HttpPost("CheckAvailability")]
        public async Task<IActionResult> CheckAvailability([FromBody] AvailabilityRequest request)
        {
            try
            {
                var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
                var services = await _context.Services
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .ToListAsync();

                var totalQuantity = request.Services.Sum(s => s.Quantity);
                var maxDuration = 0;

                foreach (var reqService in request.Services)
                {
                    var service = services.FirstOrDefault(s => s.ServiceId == reqService.ServiceId);
                    if (service == null) continue;
                    maxDuration = Math.Max(maxDuration, service.Duration);
                }

                var startTime = DateTime.Parse($"{request.Date} {request.Time}");
                var endTime = startTime.AddMinutes(maxDuration);

                var conflictingChairs = await _context.AppointmentChairs
                    .Include(ac => ac.Appointment)
                    .Where(ac => ac.Appointment.StartTime < endTime && ac.Appointment.EndTime > startTime)
                    .Select(ac => ac.ChairId)
                    .ToListAsync();

                var availableChairsCount = await _context.Chairs
                    .CountAsync(c => !conflictingChairs.Contains(c.ChairId) && c.IsAvailable);

                return Ok(new
                {
                    success = true,
                    available = availableChairsCount >= totalQuantity,
                    requiredChairs = totalQuantity,
                    availableChairs = availableChairsCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra ghế trống");
                return StatusCode(500, new { success = false, message = "Lỗi khi kiểm tra ghế trống" });
            }
        }

        // Kiểm tra vai trò admin (giữ nguyên)
        [Authorize]
        [HttpGet("Role/IsAdmin")]
        public IActionResult IsAdmin()
        {
            var isAdmin = User.IsInRole("Admin");
            return Ok(new { isAdmin });
        }

        private List<object> GetServicesList(ICollection<AppointmentService> appointmentServices)
        {
            return appointmentServices?.Select(s => new
            {
                ServiceId = s.ServiceId,
                ServiceName = s.Service?.ServiceName ?? "Dịch vụ không xác định",
                Description = s.Service?.Description ?? "Không có mô tả",
                Price = s.Price,
                Quantity = s.Quantity
            }).Cast<object>().ToList() ?? new List<object>();
        }

        private List<object> GetChairsList(ICollection<AppointmentChair> appointmentChairs)
        {
            return appointmentChairs?.Select(c => new
            {
                ChairId = c.Chair?.ChairId,
                ChairName = c.Chair?.ChairName
            }).Cast<object>().ToList() ?? new List<object>();
        }

        private List<object> GetRoomsList(ICollection<AppointmentChair> appointmentChairs)
        {
            return appointmentChairs?.Select(c => new
            {
                RoomId = c.Chair?.Room?.RoomId,
                RoomName = c.Chair?.Room?.RoomName
            }).Cast<object>().Distinct().ToList() ?? new List<object>();
        }
    }

    public class BookingRequest
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public List<ServiceRequest> Services { get; set; }
        [Required]
        public string BookingDate { get; set; }
        [Required]
        public string BookingTime { get; set; }
        public string Notes { get; set; }
    }

    public class ServiceRequest
    {
        [Required]
        public int ServiceId { get; set; }
        [Required]
        [Range(1, 10)]
        public int Quantity { get; set; }
    }

    public class AvailabilityRequest
    {
        [Required]
        public List<ServiceRequest> Services { get; set; }
        [Required]
        public string Date { get; set; }
        [Required]
        public string Time { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

    // Cập nhật model mới cho đặt lịch
    public class BookingCreateModel
    {
        public DateTime StartTime { get; set; }
        public int Duration { get; set; } // Thời gian dịch vụ tính bằng phút
        public string Notes { get; set; }
        public List<ServiceRequest> Services { get; set; }
    }
}