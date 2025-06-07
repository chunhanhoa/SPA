using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QL_Spa.Data;
using QL_Spa.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Restrict access to admins only
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
        public async Task<IActionResult> GetRooms()
        {
            try
            {
                _logger.LogInformation("Retrieving all rooms");
                
                var rooms = await _context.Rooms
                    .OrderBy(r => r.RoomId)
                    .ToListAsync();

                var result = new List<object>();
                
                foreach (var room in rooms)
                {
                    // Load chairs for this room separately
                    var chairs = await _context.Chairs
                        .Where(c => c.RoomId == room.RoomId)
                        .ToListAsync();
                    
                    result.Add(new
                    {
                        roomId = room.RoomId,
                        roomName = room.RoomName,
                        // Remove reference to Capacity property as it doesn't exist in Room model
                        isAvailable = room.IsAvailable,
                        chairsCount = chairs.Count,
                        availableChairsCount = chairs.Count(c => c.IsAvailable)
                    });
                }

                _logger.LogInformation($"Retrieved {rooms.Count} rooms");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms");
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                // Return error response
                return StatusCode(500, new { error = innerMessage });
            }
        }

        // GET: api/RoomApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRoom(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving room with ID: {id}");
                
                var room = await _context.Rooms
                    .Where(r => r.RoomId == id)
                    .Select(r => new 
                    {
                        r.RoomId,
                        r.RoomName,
                        r.IsAvailable
                    })
                    .FirstOrDefaultAsync();

                if (room == null)
                {
                    _logger.LogWarning($"Room with ID {id} not found");
                    return NotFound($"Room with ID {id} not found");
                }

                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving room with ID {id}");
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Internal server error: {innerMessage}");
            }
        }

        // POST: api/RoomApi
        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom(Room room)
        {
            try
            {
                if (room == null)
                {
                    _logger.LogWarning("Null room object received");
                    return BadRequest("Room data is required");
                }
                
                if (string.IsNullOrWhiteSpace(room.RoomName))
                {
                    _logger.LogWarning("Room name is required");
                    return BadRequest("Room name is required");
                }
                
                // Log the incoming data
                _logger.LogInformation("Creating new room: Name={RoomName}, IsAvailable={IsAvailable}", 
                    room.RoomName, room.IsAvailable);
                
                int newRoomId;
                
                // Use direct SQL to ensure the values are correctly inserted
                string sql = @"
                    INSERT INTO Rooms (RoomName, IsAvailable) 
                    VALUES (@RoomName, @IsAvailable);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    
                    // Add parameters to prevent SQL injection
                    var roomNameParam = command.CreateParameter();
                    roomNameParam.ParameterName = "@RoomName";
                    roomNameParam.Value = room.RoomName;
                    command.Parameters.Add(roomNameParam);
                    
                    var isAvailableParam = command.CreateParameter();
                    isAvailableParam.ParameterName = "@IsAvailable";
                    isAvailableParam.Value = room.IsAvailable ? 1 : 0; // Explicitly convert to 1/0
                    command.Parameters.Add(isAvailableParam);
                    
                    if (command.Connection.State != ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    // Execute the command and get the new ID
                    newRoomId = (int)await command.ExecuteScalarAsync();
                }
                
                // Get the created room from the database to confirm it was inserted correctly
                var createdRoom = await _context.Rooms.FindAsync(newRoomId);
                
                _logger.LogInformation("Room created with ID: {RoomId}, Name: {RoomName}, IsAvailable: {IsAvailable}", 
                    createdRoom.RoomId, createdRoom.RoomName, createdRoom.IsAvailable);
                
                return CreatedAtAction(nameof(GetRoom), new { id = createdRoom.RoomId }, createdRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Internal server error: {innerMessage}");
            }
        }

        // PUT: api/RoomApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, Room room)
        {
            try
            {
                if (room == null)
                {
                    _logger.LogWarning("Null room object received");
                    return BadRequest("Room data is required");
                }
                
                if (id != room.RoomId)
                {
                    _logger.LogWarning("ID mismatch: URL ID {UrlId} doesn't match body ID {BodyId}", id, room.RoomId);
                    return BadRequest("ID mismatch between URL and body");
                }

                _logger.LogInformation("Updating room with ID: {RoomId}", id);
                
                var existingRoom = await _context.Rooms.FindAsync(id);
                if (existingRoom == null)
                {
                    _logger.LogWarning("Room with ID {RoomId} not found during update", id);
                    return NotFound($"Room with ID {id} not found");
                }
                
                // Update properties
                existingRoom.RoomName = room.RoomName;
                existingRoom.IsAvailable = room.IsAvailable;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Room updated successfully");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room with ID {RoomId}", id);
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Internal server error: {innerMessage}");
            }
        }

        // DELETE: api/RoomApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                _logger.LogInformation("Deleting room with ID: {RoomId}", id);
                
                // Check if the room exists
                string checkQuery = $"SELECT COUNT(1) FROM Rooms WHERE RoomId = {id}";
                int count = 0;
                
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = checkQuery;
                    
                    if (command.Connection.State != ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
                
                if (count == 0)
                {
                    _logger.LogWarning("Room with ID {RoomId} not found during deletion", id);
                    return NotFound($"Room with ID {id} not found");
                }
                
                // Delete the room
                string query = $"DELETE FROM Rooms WHERE RoomId = {id}";
                
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    
                    if (command.Connection.State != ConnectionState.Open)
                        await command.Connection.OpenAsync();
                        
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _logger.LogInformation("Room deleted successfully. Rows affected: {RowsAffected}", rowsAffected);
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with ID {RoomId}", id);
                string innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Internal server error: {innerMessage}");
            }
        }

        private async Task<List<string>> GetTableColumns(string tableName)
        {
            var columns = new List<string>();
            
            string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                
                if (command.Connection.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();
                    
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        columns.Add(result.GetString(0));
                    }
                }
            }
            
            return columns;
        }

        private async Task AddMissingColumnsIfNeeded()
        {
            _logger.LogInformation("Checking for missing columns in Rooms table");
            
            // Add the missing columns if they don't exist
            string query = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'RoomType')
                BEGIN
                    ALTER TABLE Rooms ADD RoomType NVARCHAR(50) NULL DEFAULT 'Standard';
                    PRINT 'Added RoomType column';
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'Price')
                BEGIN
                    ALTER TABLE Rooms ADD Price DECIMAL(18, 2) NOT NULL DEFAULT 0;
                    PRINT 'Added Price column';
                END
            ";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                
                if (command.Connection.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();
                    
                await command.ExecuteNonQueryAsync();
            }
            
            _logger.LogInformation("Finished checking for missing columns");
        }

        private bool RoomExists(int id)
        {
            try
            {
                string query = $"SELECT COUNT(1) FROM Rooms WHERE RoomId = {id}";
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    
                    if (command.Connection.State != ConnectionState.Open)
                        command.Connection.Open();
                        
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result) > 0;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
