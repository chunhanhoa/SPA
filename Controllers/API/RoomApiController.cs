using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoanSpa.Data;
using LoanSpa.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using QL_Spa.Models;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomApiController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<RoomApiController> _logger;

        public RoomApiController(SpaDbContext context, ILogger<RoomApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/RoomApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            try
            {
                _logger.LogInformation("Retrieving all rooms");
                var rooms = await _context.Rooms.ToListAsync();
                _logger.LogInformation($"Retrieved {rooms.Count} rooms");
                return rooms;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms");
                return StatusCode(500, "Internal server error while retrieving rooms");
            }
        }

        // GET: api/RoomApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        // POST: api/RoomApi
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.RoomId }, room);
        }

        // PUT: api/RoomApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Room room)
        {
            if (id != room.RoomId)
            {
                return BadRequest();
            }

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/RoomApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
    }
}
