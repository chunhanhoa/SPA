using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace QL_Spa.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactApiController : ControllerBase
    {
        private readonly ILogger<ContactApiController> _logger;

        public ContactApiController(ILogger<ContactApiController> logger)
        {
            _logger = logger;
        }

        // POST: api/ContactApi/Submit
        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Contact form submitted by {Name} with email {Email}", 
                    model.FullName, model.Email);

                // Here you would typically:
                // 1. Save the contact form to a database
                // 2. Send a notification email to administrators
                // 3. Send a confirmation email to the user

                // For demo purposes, we'll just log it and return success
                await Task.Delay(500); // Simulate processing time

                return Ok(new { 
                    success = true, 
                    message = "Cảm ơn bạn đã liên hệ với chúng tôi! Chúng tôi sẽ phản hồi trong thời gian sớm nhất." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Đã xảy ra lỗi khi gửi biểu mẫu. Vui lòng thử lại sau." 
                });
            }
        }
    }

    public class ContactFormViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool AgreeTerms { get; set; }
    }
}
