using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace QL_Spa.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class InvoiceManagementController : Controller
    {
        private readonly ILogger<InvoiceManagementController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public InvoiceManagementController(ILogger<InvoiceManagementController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Admin/InvoiceManagement
        public IActionResult Index()
        {
            _logger.LogInformation("Accessed Invoice Management page");
            return View("~/Views/Admin/InvoiceManagement.cshtml");
        }

        // POST: Admin/InvoiceManagement/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int invoiceId, string status)
        {
            _logger.LogInformation($"Updating invoice {invoiceId} status to {status}");
            
            try
            {
                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                
                // Prepare request data
                var requestData = new { Status = status };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");
                
                // Send request to API
                var response = await client.PostAsync($"/api/Invoice/{invoiceId}/UpdateStatus", content);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Trạng thái hóa đơn đã được cập nhật thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái hóa đơn";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating invoice {invoiceId} status");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái hóa đơn";
            }
            
            return RedirectToAction("Index");
        }
    }
}