using LoanSpa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private static List<Booking> _bookings = new List<Booking>();
        private static int _lastId = 0;

        // GET: api/Bookings
        [HttpGet]
        [Authorize] // Yêu cầu đăng nhập để xem tất cả các đơn đặt lịch (Admin)
        public ActionResult<IEnumerable<Booking>> GetBookings()
        {
            // Trong thực tế, bạn sẽ phải kiểm tra role là admin
            return Ok(_bookings);
        }

        // GET: api/Bookings/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize] // Yêu cầu đăng nhập
        public ActionResult<IEnumerable<Booking>> GetUserBookings(string userId)
        {
            var userBookings = _bookings.Where(b => b.UserId == userId).ToList();
            return Ok(userBookings);
        }

        // POST: api/Bookings
        [HttpPost]
        public ActionResult<Booking> CreateBooking(Booking booking)
        {
            booking.Id = ++_lastId;
            booking.CreatedAt = DateTime.Now;
            booking.Status = "Chờ xác nhận";
            
            _bookings.Add(booking);
            
            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public ActionResult<Booking> GetBookingById(int id)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == id);
            
            if (booking == null)
            {
                return NotFound();
            }
            
            return booking;
        }
        
        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        [Authorize] // Yêu cầu đăng nhập để cập nhật
        public IActionResult UpdateBooking(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return BadRequest();
            }
            
            var existingBooking = _bookings.FirstOrDefault(b => b.Id == id);
            if (existingBooking == null)
            {
                return NotFound();
            }
            
            // Cập nhật thông tin đặt lịch
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.BookingTime = booking.BookingTime;
            existingBooking.Status = booking.Status;
            existingBooking.Note = booking.Note;
            
            return NoContent();
        }
        
        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        [Authorize] // Yêu cầu đăng nhập để xoá
        public IActionResult DeleteBooking(int id)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }
            
            _bookings.Remove(booking);
            return NoContent();
        }
    }
}
