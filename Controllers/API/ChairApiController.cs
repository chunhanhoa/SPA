using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using QL_Spa.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Restrict access to admins only
    public class ChairApiController : ControllerBase
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<ChairApiController> _logger;

        public ChairApiController(SpaDbContext context, ILogger<ChairApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ChairApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetChairs()
        {
            try
            {
                _logger.LogInformation("Retrieving all chairs");

                var chairs = await _context.Chairs
                    .Include(c => c.Room)
                    .Select(c => new
                    {
                        c.ChairId,
                        c.ChairName,
                        c.IsAvailable,
                        c.RoomId,
                        RoomName = c.Room.RoomName
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {chairs.Count} chairs");
                return chairs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chairs");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ChairApi/test
        [HttpGet("test")]
        public ActionResult<object> TestConnection()
        {
            try
            {
                // Test database connection
                bool canConnect = _context.Database.CanConnect();
                
                // Count tables
                int roomCount = _context.Rooms.Count();
                int chairCount = _context.Chairs.Count();
                
                return Ok(new { 
                    Message = "API is working", 
                    DatabaseConnection = canConnect,
                    RoomCount = roomCount,
                    ChairCount = chairCount,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint");
                return StatusCode(500, new { Error = ex.Message, Details = ex.StackTrace });
            }
        }

        // POST: api/ChairApi/directsql
        [HttpPost("directsql")]
        public async Task<IActionResult> DirectSqlInsert([FromBody] JsonElement data)
        {
            try
            {
                _logger.LogInformation("Received direct SQL insert request: {Data}", data.ToString());
                
                // Extract the values
                string chairName = "Test Chair";
                int roomId = 1;
                bool isAvailable = true;
                
                if (data.TryGetProperty("chairName", out var nameElement))
                    chairName = nameElement.GetString();
                    
                if (data.TryGetProperty("roomId", out var roomElement))
                    roomId = roomElement.GetInt32();
                    
                if (data.TryGetProperty("isAvailable", out var availableElement))
                    isAvailable = availableElement.GetBoolean();
                
                // Execute direct SQL
                string sql = @"
                    INSERT INTO Chairs (ChairName, IsAvailable, RoomId) 
                    VALUES (@p0, @p1, @p2);";
                    
                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    sql, 
                    chairName, 
                    isAvailable, 
                    roomId);
                    
                return Ok(new {
                    Success = true,
                    RowsAffected = rowsAffected,
                    Message = $"Directly inserted chair '{chairName}' in room {roomId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct SQL insert");
                return StatusCode(500, new { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        // GET: api/ChairApi/ByRoom/5
        [HttpGet("ByRoom/{roomId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetChairsByRoom(int roomId)
        {
            try
            {
                _logger.LogInformation($"Retrieving chairs for room ID: {roomId}");

                var chairs = await _context.Chairs
                    .Where(c => c.RoomId == roomId)
                    .Include(c => c.Room)
                    .Select(c => new
                    {
                        c.ChairId,
                        c.ChairName,
                        c.IsAvailable,
                        c.RoomId,
                        RoomName = c.Room.RoomName
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {chairs.Count} chairs for room ID: {roomId}");
                return chairs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving chairs for room ID: {roomId}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ChairApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetChair(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving chair with ID: {id}");

                var chair = await _context.Chairs
                    .Include(c => c.Room)
                    .Where(c => c.ChairId == id)
                    .Select(c => new
                    {
                        c.ChairId,
                        c.ChairName,
                        c.IsAvailable,
                        c.RoomId,
                        RoomName = c.Room.RoomName
                    })
                    .FirstOrDefaultAsync();

                if (chair == null)
                {
                    _logger.LogWarning($"Chair with ID {id} not found");
                    return NotFound($"Chair with ID {id} not found");
                }

                return chair;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving chair with ID {id}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ChairApi
        [HttpPost]
        public async Task<ActionResult<object>> CreateChair([FromBody] JsonElement requestData)
        {
            try
            {
                _logger.LogInformation("Received chair creation request");
                
                // Extract values with fallbacks for both camelCase and PascalCase
                string chairName = null;
                int roomId = 0;
                bool isAvailable = true;
                
                // Try to extract chairName
                if (requestData.TryGetProperty("ChairName", out var nameElement1))
                    chairName = nameElement1.GetString();
                else if (requestData.TryGetProperty("chairName", out var nameElement2))
                    chairName = nameElement2.GetString();
                
                // Try to extract roomId
                if (requestData.TryGetProperty("RoomId", out var roomElement1))
                    roomId = roomElement1.GetInt32();
                else if (requestData.TryGetProperty("roomId", out var roomElement2))
                    roomId = roomElement2.GetInt32();
                
                // Try to extract isAvailable
                if (requestData.TryGetProperty("IsAvailable", out var availableElement1))
                    isAvailable = availableElement1.GetBoolean();
                else if (requestData.TryGetProperty("isAvailable", out var availableElement2))
                    isAvailable = availableElement2.GetBoolean();
                
                _logger.LogInformation("Extracted data: ChairName={ChairName}, RoomId={RoomId}, IsAvailable={IsAvailable}", 
                    chairName, roomId, isAvailable);
                
                // Validate inputs
                if (string.IsNullOrWhiteSpace(chairName))
                    return BadRequest("Chair name is required");
                    
                if (roomId <= 0)
                    return BadRequest("Valid Room ID is required");
                
                // Use pure ADO.NET for direct database access
                int newChairId;
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    
                    string sql = @"
                        IF EXISTS (SELECT 1 FROM Rooms WHERE RoomId = @RoomId)
                        BEGIN
                            INSERT INTO Chairs (ChairName, IsAvailable, RoomId) 
                            VALUES (@ChairName, @IsAvailable, @RoomId);
                            SELECT CAST(SCOPE_IDENTITY() as int);
                        END
                        ELSE
                        BEGIN
                            SELECT -1; -- Room doesn't exist
                        END";
                        
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        
                        var nameParam = command.CreateParameter();
                        nameParam.ParameterName = "@ChairName";
                        nameParam.Value = chairName;
                        command.Parameters.Add(nameParam);
                        
                        var availableParam = command.CreateParameter();
                        availableParam.ParameterName = "@IsAvailable";
                        availableParam.Value = isAvailable ? 1 : 0;
                        command.Parameters.Add(availableParam);
                        
                        var roomParam = command.CreateParameter();
                        roomParam.ParameterName = "@RoomId";
                        roomParam.Value = roomId;
                        command.Parameters.Add(roomParam);
                        
                        var result = await command.ExecuteScalarAsync();
                        newChairId = Convert.ToInt32(result);
                        
                        if (newChairId == -1)
                            return BadRequest($"Room with ID {roomId} not found");
                        
                        _logger.LogInformation("Chair inserted successfully with ID: {ChairId}", newChairId);
                    }
                }
                
                // Return a simple response without trying to load the chair again
                return Ok(new {
                    ChairId = newChairId,
                    ChairName = chairName,
                    RoomId = roomId,
                    IsAvailable = isAvailable
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chair: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { 
                    Error = ex.Message, 
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace 
                });
            }
        }

        // PUT: api/ChairApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChair(int id, Chair chair)
        {
            try
            {
                if (chair == null)
                {
                    _logger.LogWarning("Null chair object received");
                    return BadRequest("Chair data is required");
                }

                if (id != chair.ChairId)
                {
                    _logger.LogWarning("ID mismatch: URL ID {UrlId} doesn't match body ID {BodyId}", id, chair.ChairId);
                    return BadRequest("ID mismatch between URL and body");
                }

                _logger.LogInformation("Updating chair with ID: {ChairId}", id);

                var existingChair = await _context.Chairs.FindAsync(id);
                if (existingChair == null)
                {
                    _logger.LogWarning("Chair with ID {ChairId} not found during update", id);
                    return NotFound($"Chair with ID {id} not found");
                }

                // Check if room exists
                if (chair.RoomId > 0)
                {
                    var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == chair.RoomId);
                    if (!roomExists)
                    {
                        _logger.LogWarning($"Room with ID {chair.RoomId} not found");
                        return BadRequest($"Room with ID {chair.RoomId} not found");
                    }
                }

                // Update properties
                existingChair.ChairName = chair.ChairName;
                existingChair.IsAvailable = chair.IsAvailable;
                if (chair.RoomId > 0)
                {
                    existingChair.RoomId = chair.RoomId;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Chair updated successfully");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chair with ID {ChairId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ChairApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChair(int id)
        {
            try
            {
                _logger.LogInformation("Deleting chair with ID: {ChairId}", id);

                var chair = await _context.Chairs.FindAsync(id);
                if (chair == null)
                {
                    _logger.LogWarning($"Chair with ID {id} not found during deletion");
                    return NotFound($"Chair with ID {id} not found");
                }

                _context.Chairs.Remove(chair);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Chair deleted successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chair with ID {ChairId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Add this method for simple debugging
        [HttpGet("simple-test")]
        [AllowAnonymous] // Allow access without authentication for testing
        public ActionResult<object> SimpleTest()
        {
            try
            {
                return Ok(new {
                    Message = "API is working",
                    Timestamp = DateTime.Now,
                    Status = "Success"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // Add a simplified chair creation endpoint for testing
        [HttpGet("create-test-chair")]
        public async Task<ActionResult<object>> CreateTestChair()
        {
            try
            {
                // Find a valid room ID
                int roomId = 0;
                var firstRoom = await _context.Rooms.FirstOrDefaultAsync();
                if (firstRoom != null)
                {
                    roomId = firstRoom.RoomId;
                }
                else
                {
                    // Create a test room if none exists
                    var newRoom = new Room { 
                        RoomName = "Test Room " + DateTime.Now.Ticks,
                        IsAvailable = true
                    };
                    _context.Rooms.Add(newRoom);
                    await _context.SaveChangesAsync();
                    roomId = newRoom.RoomId;
                }
                
                // Create a test chair
                var chair = new Chair {
                    ChairName = "Test Chair " + DateTime.Now.Ticks,
                    RoomId = roomId,
                    IsAvailable = true
                };
                
                _context.Chairs.Add(chair);
                await _context.SaveChangesAsync();
                
                return Ok(new {
                    Success = true,
                    Message = "Test chair created successfully",
                    ChairId = chair.ChairId,
                    ChairName = chair.ChairName,
                    RoomId = chair.RoomId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = ex.Message, 
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace 
                });
            }
        }
    }
}
