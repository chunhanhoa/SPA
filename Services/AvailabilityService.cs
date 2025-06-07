using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QL_Spa.Services
{
    public class AvailabilityService
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(SpaDbContext context, ILogger<AvailabilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Chair>> GetAvailableChairsAsync(DateTime startTime, DateTime endTime, int count)
        {
            try
            {
                // Lấy danh sách ghế đã được đặt trong khoảng thời gian này
                var bookedChairIds = await _context.AppointmentChairs
                    .Include(ac => ac.Appointment)
                    .Where(ac => ac.Appointment.StartTime < endTime && ac.Appointment.EndTime > startTime)
                    .Select(ac => ac.ChairId)
                    .ToListAsync();

                // Lấy danh sách ghế còn trống
                var availableChairs = await _context.Chairs
                    .Where(c => !bookedChairIds.Contains(c.ChairId) && c.IsAvailable)
                    .Take(count)
                    .ToListAsync();

                return availableChairs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available chairs");
                throw;
            }
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // Lấy tất cả ghế trong phòng
                var chairIds = await _context.Chairs
                    .Where(c => c.RoomId == roomId)
                    .Select(c => c.ChairId)
                    .ToListAsync();

                if (!chairIds.Any())
                {
                    return false; // Phòng không có ghế nào
                }

                // Lấy danh sách ghế đã được đặt trong khoảng thời gian này
                var bookedChairCount = await _context.AppointmentChairs
                    .Include(ac => ac.Appointment)
                    .Where(ac => 
                        chairIds.Contains(ac.ChairId) && 
                        ac.Appointment.StartTime < endTime && 
                        ac.Appointment.EndTime > startTime)
                    .CountAsync();

                // Nếu số ghế đã đặt ít hơn tổng số ghế, phòng còn trống
                return bookedChairCount < chairIds.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking room {roomId} availability");
                throw;
            }
        }
    }
}
