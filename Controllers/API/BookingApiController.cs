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
                
                // Cập nhật trạng thái ghế khi trạng thái lịch hẹn thay đổi
                await UpdateChairAvailabilityAsync(id, request.Status);
                
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
                        Status = appointment.Status ?? "Đã xác nhận",
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
                    
                    // Nếu có thông tin khách hàng mới, cập nhật
                    if (customer != null && 
                        (!string.IsNullOrEmpty(model.CustomerName) || !string.IsNullOrEmpty(model.CustomerPhone)))
                    {
                        if (!string.IsNullOrEmpty(model.CustomerName))
                            customer.FullName = model.CustomerName;
                        
                        if (!string.IsNullOrEmpty(model.CustomerPhone))
                            customer.Phone = model.CustomerPhone;
                        
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã cập nhật thông tin khách hàng ID: {ID}", customer.CustomerId);
                    }
                }

                // Nếu không tìm thấy khách hàng, kiểm tra nếu có thông tin khách hàng trong request
                if (customer == null && !string.IsNullOrEmpty(model.CustomerPhone))
                {
                    // Tìm khách hàng theo số điện thoại
                    customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == model.CustomerPhone);
                    
                    // Nếu tìm thấy khách hàng theo số điện thoại, cập nhật tên
                    if (customer != null && !string.IsNullOrEmpty(model.CustomerName))
                    {
                        customer.FullName = model.CustomerName;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã cập nhật tên khách hàng ID: {ID}", customer.CustomerId);
                    }
                }

                // Nếu vẫn không tìm thấy khách hàng và có thông tin, tạo mới
                if (customer == null && !string.IsNullOrEmpty(model.CustomerName) && !string.IsNullOrEmpty(model.CustomerPhone))
                {
                    customer = new Customer
                    {
                        FullName = model.CustomerName,
                        Phone = model.CustomerPhone,
                        UserId = userId, // Liên kết với tài khoản nếu đã đăng nhập
                        CreatedDate = DateTime.Now,
                        TotalAmount = 0
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Đã tạo khách hàng mới ID: {ID}", customer.CustomerId);
                }

                // Nếu không có thông tin khách hàng, sử dụng khách hàng mặc định
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
                    // Đảm bảo StartTime được xử lý như một giờ địa phương
                    StartTime = DateTime.Parse(model.StartTime),
                    // Tính EndTime từ StartTime
                    EndTime = DateTime.Parse(model.StartTime).AddMinutes(model.Duration),
                    TotalAmount = totalAmount,
                    Status = "Chờ xác nhận", // Changed from "Đã xác nhận" to "Chờ xác nhận"
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

                // Tìm ghế trống và gán cho lịch hẹn - Sử dụng CustomerCount thay vì totalQuantity
                int chairsNeeded = Math.Max(1, model.CustomerCount); // Đảm bảo ít nhất 1 ghế
                _logger.LogInformation($"Đang tìm {chairsNeeded} ghế cho {model.CustomerCount} khách hàng");
                
                var availableChairs = await _context.Chairs
                    .Where(c => c.IsAvailable)
                    .Take(chairsNeeded)
                    .ToListAsync();

                if (availableChairs.Count < chairsNeeded)
                {
                    // Nếu không đủ ghế, vẫn cho đặt lịch nhưng ghi chú lại
                    appointment.Notes += $" (Thiếu ghế, cần {chairsNeeded} ghế nhưng chỉ có {availableChairs.Count})";
                    _logger.LogWarning($"Không đủ ghế trống. Cần: {chairsNeeded}, Có sẵn: {availableChairs.Count}");
                }

                // Gán ghế và cập nhật trạng thái
                foreach (var chair in availableChairs)
                {
                    appointment.AppointmentChairs.Add(new AppointmentChair
                    {
                        ChairId = chair.ChairId
                    });
                    
                    // Cập nhật trạng thái ghế thành đã đặt
                    chair.IsAvailable = false;
                    _logger.LogInformation($"Đã cập nhật ghế {chair.ChairId} - {chair.ChairName} thành không còn trống");
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo lịch hẹn mới với ID: {ID}", appointment.AppointmentId);
                
                // Tự động tạo hóa đơn cho lịch hẹn
                try
                {
                    _logger.LogInformation("Bắt đầu tạo hóa đơn tự động cho lịch hẹn ID: {ID}", appointment.AppointmentId);
                    
                    // Create new invoice with all required fields set
                    var invoice = new Invoice
                    {
                        CreatedDate = DateTime.Now,
                        TotalAmount = appointment.TotalAmount,
                        Discount = 0, // Mặc định giảm giá 0
                        PaidAmount = 0, // Set default to 0, can be updated later
                        CustomerId = appointment.CustomerId,
                        Status = "Chờ thanh toán", // Set status as requested
                        InvoiceServices = new List<InvoiceService>()
                    };

                    _logger.LogInformation("Tạo hóa đơn với: TotalAmount={TotalAmount}, Discount={Discount}, CustomerId={CustomerId}, Status={Status}", 
                        invoice.TotalAmount, invoice.Discount, invoice.CustomerId, invoice.Status);

                    // Add services from appointment to invoice
                    foreach (var appService in appointment.AppointmentServices)
                    {
                        var invoiceService = new InvoiceService
                        {
                            ServiceId = appService.ServiceId,
                            Quantity = appService.Quantity,
                            Price = appService.Price
                        };
                        
                        _logger.LogInformation("Thêm dịch vụ vào hóa đơn: ServiceId={ServiceId}, Quantity={Quantity}, Price={Price}", 
                            invoiceService.ServiceId, invoiceService.Quantity, invoiceService.Price);
                            
                        invoice.InvoiceServices.Add(invoiceService);
                    }

                    // Use direct SQL to insert the invoice if EF Core approach is failing
                    // This will help bypass any EF Core configuration issues
                    string insertInvoiceSql = @"
                        INSERT INTO Invoices (CreatedDate, TotalAmount, Discount, PaidAmount, CustomerId, Status)
                        VALUES (@createdDate, @totalAmount, @discount, @paidAmount, @customerId, @status);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
                        
                    var parameters = new[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@createdDate", invoice.CreatedDate),
                        new Microsoft.Data.SqlClient.SqlParameter("@totalAmount", invoice.TotalAmount),
                        new Microsoft.Data.SqlClient.SqlParameter("@discount", invoice.Discount),
                        new Microsoft.Data.SqlClient.SqlParameter("@paidAmount", invoice.PaidAmount),
                        new Microsoft.Data.SqlClient.SqlParameter("@customerId", invoice.CustomerId),
                        new Microsoft.Data.SqlClient.SqlParameter("@status", invoice.Status)
                    };
                    
                    // Execute the SQL and get the generated invoice ID
                    var invoiceId = await _context.Database.ExecuteSqlRawAsync(insertInvoiceSql, parameters);
                    
                    if (invoiceId > 0)
                    {
                        _logger.LogInformation("Hóa đơn đã được tạo với ID: {ID}", invoiceId);
                        
                        // Now insert the invoice services
                        foreach (var service in invoice.InvoiceServices)
                        {
                            string insertServiceSql = @"
                                INSERT INTO InvoiceServices (InvoiceId, ServiceId, Quantity, Price)
                                VALUES (@invoiceId, @serviceId, @quantity, @price);";
                                
                            var serviceParams = new[] {
                                new Microsoft.Data.SqlClient.SqlParameter("@invoiceId", invoiceId),
                                new Microsoft.Data.SqlClient.SqlParameter("@serviceId", service.ServiceId),
                                new Microsoft.Data.SqlClient.SqlParameter("@quantity", service.Quantity),
                                new Microsoft.Data.SqlClient.SqlParameter("@price", service.Price)
                            };
                            
                            await _context.Database.ExecuteSqlRawAsync(insertServiceSql, serviceParams);
                        }
                        
                        _logger.LogInformation("Các dịch vụ đã được thêm vào hóa đơn ID: {ID}", invoiceId);
                    }
                    else
                    {
                        _logger.LogWarning("Không thể tạo hóa đơn bằng SQL trực tiếp, thử dùng EF Core");
                        
                        // Fall back to EF Core if direct SQL fails
                        _context.Invoices.Add(invoice);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã tạo hóa đơn thành công với EF Core, ID: {ID}", invoice.InvoiceId);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the appointment creation
                    _logger.LogError(ex, "Lỗi khi tạo hóa đơn tự động cho lịch hẹn ID: {ID}: {Message}", 
                        appointment.AppointmentId, ex.Message);
                }
                
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

                // Sử dụng CustomerCount thay vì tổng số dịch vụ
                var chairsNeeded = Math.Max(1, request.CustomerCount);
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
                    available = availableChairsCount >= chairsNeeded,
                    requiredChairs = chairsNeeded,
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

        // Fix the GetRoomsList function to properly handle chairs from multiple rooms
        private List<object> GetRoomsList(ICollection<AppointmentChair> appointmentChairs)
        {
            // Use a Dictionary to track unique rooms
            var uniqueRooms = new Dictionary<int, object>();
            
            if (appointmentChairs != null)
            {
                foreach (var chair in appointmentChairs)
                {
                    if (chair.Chair?.Room != null)
                    {
                        var roomId = chair.Chair.Room.RoomId;
                        
                        // Add room to dictionary if it's not already there
                        if (!uniqueRooms.ContainsKey(roomId))
                        {
                            uniqueRooms[roomId] = new
                            {
                                RoomId = roomId,
                                RoomName = chair.Chair.Room.RoomName,
                                Chairs = new List<string>() // Initialize empty chairs list
                            };
                        }
                        
                        // Add chair to the list of chairs for this room
                        ((dynamic)uniqueRooms[roomId]).Chairs.Add(chair.Chair.ChairName);
                    }
                }
            }
            
            _logger.LogInformation($"Found {uniqueRooms.Count} unique rooms in the booking");
            foreach (var room in uniqueRooms.Values)
            {
                _logger.LogInformation($"Room: {((dynamic)room).RoomName}, Chairs: {string.Join(", ", ((dynamic)room).Chairs)}");
            }
            
            return uniqueRooms.Values.Cast<object>().ToList();
        }

        // Lấy lịch hẹn của người dùng
        [HttpGet("user")]
        public async Task<IActionResult> GetUserBookings()
        {
            try
            {
                _logger.LogInformation("Đang lấy danh sách lịch đặt từ endpoint /user");
                // Lấy ID người dùng hiện tại
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Không tìm thấy UserId trong claims");
                    return Unauthorized(new { success = false, message = "Chưa đăng nhập" });
                }

                _logger.LogInformation($"Tìm lịch hẹn cho userId: {userId}");
                
                // Lấy thông tin khách hàng
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                    
                if (customer == null)
                {
                    _logger.LogWarning($"Không tìm thấy thông tin khách hàng cho userId: {userId}");
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
                }
                
                _logger.LogInformation($"Tìm thấy khách hàng với CustomerId: {customer.CustomerId}");

                // Lấy tất cả lịch hẹn của khách hàng
                var bookings = await _context.Appointments
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .Include(a => a.AppointmentChairs)
                        .ThenInclude(apc => apc.Chair)
                            .ThenInclude(c => c.Room)
                    .Where(a => a.CustomerId == customer.CustomerId)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                _logger.LogInformation($"Tìm thấy {bookings.Count} lịch hẹn cho khách hàng {customer.CustomerId}");

                // Format dữ liệu kết quả tương tự như GetMyBookings()
                var result = bookings.Select(appointment => new
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch đặt của người dùng");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi lấy lịch hẹn" });
            }
        }

        // Method to update chair availability based on appointment status
        private async Task UpdateChairAvailabilityAsync(int appointmentId, string status)
        {
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentChairs)
                .ThenInclude(ac => ac.Chair)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            
            if (appointment != null)
            {
                // If appointment is cancelled or completed, release the chairs
                if (status == "Đã hủy" || status == "Hoàn thành")
                {
                    foreach (var appointmentChair in appointment.AppointmentChairs)
                    {
                        if (appointmentChair.Chair != null)
                        {
                            appointmentChair.Chair.IsAvailable = true;
                            _logger.LogInformation($"Đã giải phóng ghế {appointmentChair.Chair.ChairId} - {appointmentChair.Chair.ChairName}");
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        // GET: api/Booking/ManagementData
        [Authorize(Roles = "Admin")]
        [HttpGet("ManagementData")]
        public async Task<IActionResult> GetBookingManagementData()
        {
            try
            {
                _logger.LogInformation("Fetching booking management data");
                
                var appointments = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                    .Include(a => a.AppointmentChairs)
                        .ThenInclude(ac => ac.Chair)
                            .ThenInclude(c => c.Room)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                var result = appointments.Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    totalAmount = a.TotalAmount,
                    startTime = a.StartTime,
                    endTime = a.EndTime,
                    createdDate = a.CreatedDate,
                    customerName = a.Customer?.FullName ?? "N/A",
                    customerPhone = a.Customer?.Phone ?? "N/A", // Add customer phone number
                    notes = a.Notes ?? "",
                    status = a.Status ?? "Chờ xác nhận",
                    serviceCount = a.AppointmentServices?.Count ?? 0,
                    services = a.AppointmentServices?.Select(s => s.Service?.ServiceName).ToList() ?? new List<string>(),
                    chairs = a.AppointmentChairs?.Select(c => new {
                        chairId = c.ChairId,
                        chairName = c.Chair?.ChairName ?? "N/A",
                        roomName = c.Chair?.Room?.RoomName ?? "N/A"
                    }).Cast<object>().ToList() ?? new List<object>()
                }).ToList();
                
                _logger.LogInformation("Successfully fetched {Count} appointments for management view", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking management data");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi lấy dữ liệu đặt lịch" });
            }
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
        public int CustomerCount { get; set; } = 1; // Thêm số lượng khách hàng, mặc định là 1
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

    // Cập nhật model mới cho đặt lịch
    public class BookingCreateModel
    {
        // Thay đổi kiểu dữ liệu để nhận chuỗi datetime từ client
        public string StartTime { get; set; }
        public int Duration { get; set; } // Thời gian dịch vụ tính bằng phút
        public int CustomerCount { get; set; } = 1; // Số lượng khách hàng, mặc định là 1
        public string Notes { get; set; }
        public List<ServiceRequest> Services { get; set; }
        
        // Thêm thông tin khách hàng
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
    }
}