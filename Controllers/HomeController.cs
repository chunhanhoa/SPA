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

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

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

    public IActionResult Booking()
    {
        ViewData["Title"] = "Đặt lịch";
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
            // Get service details from the API
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
