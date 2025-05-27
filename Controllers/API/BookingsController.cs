using LoanSpa.Data;
using LoanSpa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Bookings
        [HttpGet]
        [Authorize] // Yêu cầu đăng nhập để xem tất cả các đơn đặt lịch (Admin)
        public async Task<ActionResult<IEnumerable<Appointment>>> GetBookings()
        {
            // Trong thực tế, bạn sẽ phải kiểm tra role là admin
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .ToListAsync();
        }

        // GET: api/Bookings/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize] // Yêu cầu đăng nhập
        public async Task<ActionResult<IEnumerable<Appointment>>> GetUserBookings(string userId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng");
            }

            var userBookings = await _context.Appointments
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .Include(a => a.AppointmentChairs)
                    .ThenInclude(c => c.Chair)
                .Where(a => a.CustomerId == customer.CustomerId)
                .ToListAsync();

            return Ok(userBookings);
        }

        // POST: api/Bookings
        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateBooking(AppointmentViewModel model)
        {
            // Kiểm tra xem khách hàng có tồn tại không
            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer == null)
            {
                return BadRequest("Thông tin khách hàng không hợp lệ");
            }

            // Tạo appointment mới
            var appointment = new Appointment
            {
                CustomerId = model.CustomerId,
                StartTime = model.StartTime,
                EndTime = model.StartTime.AddMinutes(model.Duration),
                CreatedDate = DateTime.Now,
                TotalAmount = model.TotalAmount
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Thêm các dịch vụ cho appointment
            if (model.ServiceIds != null && model.ServiceIds.Any())
            {
                foreach (var serviceItem in model.ServiceIds)
                {
                    var appointmentService = new AppointmentService
                    {
                        AppointmentId = appointment.AppointmentId,
                        ServiceId = serviceItem.Key,
                        Quantity = serviceItem.Value
                    };
                    _context.AppointmentServices.Add(appointmentService);
                }
            }

            // Thêm thông tin ghế nếu có
            if (model.ChairIds != null && model.ChairIds.Any())
            {
                foreach (var chairItem in model.ChairIds)
                {
                    var appointmentChair = new AppointmentChair
                    {
                        AppointmentId = appointment.AppointmentId,
                        ChairId = chairItem.Key,
                        Quantity = chairItem.Value
                    };
                    _context.AppointmentChairs.Add(appointmentChair);
                }
            }

            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetBooking), new { id = appointment.AppointmentId }, appointment);
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetBooking(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .Include(a => a.AppointmentChairs)
                    .ThenInclude(c => c.Chair)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
            
            if (appointment == null)
            {
                return NotFound();
            }
            
            return appointment;
        }
        
        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        [Authorize] // Yêu cầu đăng nhập để cập nhật
        public async Task<IActionResult> UpdateBooking(int id, AppointmentViewModel model)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            
            if (appointment == null)
            {
                return NotFound();
            }
            
            // Cập nhật thông tin đặt lịch
            appointment.StartTime = model.StartTime;
            appointment.EndTime = model.StartTime.AddMinutes(model.Duration);
            appointment.TotalAmount = model.TotalAmount;
            
            // Cập nhật các dịch vụ
            // Đầu tiên xóa tất cả dịch vụ cũ
            var existingServices = await _context.AppointmentServices
                .Where(s => s.AppointmentId == id)
                .ToListAsync();
                
            _context.AppointmentServices.RemoveRange(existingServices);
            
            // Thêm các dịch vụ mới
            if (model.ServiceIds != null && model.ServiceIds.Any())
            {
                foreach (var serviceItem in model.ServiceIds)
                {
                    var appointmentService = new AppointmentService
                    {
                        AppointmentId = id,
                        ServiceId = serviceItem.Key,
                        Quantity = serviceItem.Value
                    };
                    _context.AppointmentServices.Add(appointmentService);
                }
            }
            
            // Cập nhật thông tin ghế
            var existingChairs = await _context.AppointmentChairs
                .Where(c => c.AppointmentId == id)
                .ToListAsync();
                
            _context.AppointmentChairs.RemoveRange(existingChairs);
            
            // Thêm thông tin ghế mới
            if (model.ChairIds != null && model.ChairIds.Any())
            {
                foreach (var chairItem in model.ChairIds)
                {
                    var appointmentChair = new AppointmentChair
                    {
                        AppointmentId = id,
                        ChairId = chairItem.Key,
                        Quantity = chairItem.Value
                    };
                    _context.AppointmentChairs.Add(appointmentChair);
                }
            }
            
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        [Authorize] // Yêu cầu đăng nhập để xoá
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }

    // Cập nhật AppointmentViewModel phù hợp với model đã scaffold
    public class AppointmentViewModel
    {
        public int CustomerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; }
        public decimal? TotalAmount { get; set; }
        
        // Thêm required hoặc đặt giá trị mặc định
        public Dictionary<int, int> ServiceIds { get; set; } = new();
        public Dictionary<int, int> ChairIds { get; set; } = new();
    }
}
