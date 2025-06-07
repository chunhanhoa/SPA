using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Data; // Add this to access SpaDbContext
using QL_Spa.Models;

namespace QL_Spa.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SpaDbContext _context; // Changed from ApplicationDbContext to SpaDbContext

    public HomeController(ILogger<HomeController> logger, SpaDbContext context) // Changed from ApplicationDbContext to SpaDbContext
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> BookingConfirmation(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .Include(a => a.AppointmentChairs)
                .ThenInclude(ac => ac.Chair)
                    .ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
        {
            return NotFound();
        }

        // Get grouped rooms and chairs
        var roomsWithChairs = appointment.AppointmentChairs
            .Where(ac => ac.Chair?.Room != null)
            .GroupBy(ac => ac.Chair.Room.RoomId)
            .Select(group => new {
                Room = group.First().Chair.Room,
                Chairs = group.Select(ac => ac.Chair.ChairName).ToList()
            })
            .ToList();

        ViewBag.RoomsWithChairs = roomsWithChairs;

        return View(appointment);
    }

    // Các action khác giữ nguyên
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();
    public IActionResult About()
    {
        ViewData["Title"] = "Về chúng tôi";
        return View();
    }
    
    public IActionResult Contact()
    {
        ViewData["Title"] = "Liên hệ";
        return View();
    }

    public IActionResult Booking(int? serviceId)
    {
        ViewData["Title"] = "Đặt lịch";
        if (serviceId.HasValue)
        {
            ViewData["ServiceId"] = serviceId.Value.ToString();
            _logger.LogInformation($"Đang tải trang đặt lịch với dịch vụ ID: {serviceId.Value}");
        }
        else
        {
            _logger.LogInformation("Đang tải trang đặt lịch không có dịch vụ được chọn trước");
        }
        return View();
    }

    public IActionResult Services()
    {
        ViewData["Title"] = "Dịch vụ";
        return View();
    }

    public async Task<IActionResult> ServiceDetails(int id)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                string apiUrl = $"{Request.Scheme}://{Request.Host}/api/ServiceApi/{id}";
                var response = await httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var service = System.Text.Json.JsonSerializer.Deserialize<Service>(
                        jsonString, 
                        new System.Text.Json.JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        }
                    );
                    return View(service);
                }
                else
                {
                    _logger.LogWarning("Service with ID {ServiceId} not found", id);
                    return View(null);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service details for ID {ServiceId}", id);
            return View(null);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}