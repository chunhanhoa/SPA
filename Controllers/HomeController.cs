using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QL_Spa.Models;

namespace QL_Spa.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult BookingConfirmation(int id)
    {
        ViewData["Title"] = "Xác nhận đặt lịch";
        
        if (id <= 0)
        {
            _logger.LogWarning("Truy cập trang xác nhận đặt lịch với ID không hợp lệ: {ID}", id);
            return BadRequest("ID lịch hẹn không hợp lệ");
        }
        
        _logger.LogInformation("Đang tải trang xác nhận đặt lịch ID: {ID}", id);
        return View(id);
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