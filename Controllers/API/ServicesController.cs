using LoanSpa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace LoanSpa.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private static readonly Dictionary<string, Service> _services = new Dictionary<string, Service>
        {
            {
                "massage", new Service
                {
                    Id = "massage",
                    Name = "Massage Trị Liệu",
                    Image = "/images/services/massage.jpg",
                    Description = "Dịch vụ Massage Trị Liệu tại LoanSpa là sự kết hợp giữa các kỹ thuật massage truyền thống và hiện đại.",
                    Price = 500000,
                    Duration = "60 phút",
                    Benefits = new List<string>
                    {
                        "Giảm căng thẳng và mệt mỏi",
                        "Cải thiện tuần hoàn máu",
                        "Giảm đau nhức cơ bắp",
                        "Tăng cường hệ miễn dịch",
                        "Cải thiện giấc ngủ"
                    },
                    Process = new List<string>
                    {
                        "Tư vấn và đánh giá tình trạng sức khỏe",
                        "Thư giãn với trà thảo mộc và khăn ấm",
                        "Massage toàn thân với dầu tinh chất tự nhiên",
                        "Tập trung vào các vùng căng cơ theo yêu cầu",
                        "Thư giãn và nghỉ ngơi"
                    },
                    Note = "Không áp dụng cho phụ nữ mang thai 3 tháng đầu, người có vấn đề về tim mạch."
                }
            },
            {
                "facial", new Service
                {
                    Id = "facial",
                    Name = "Chăm Sóc Da Mặt",
                    Image = "/images/services/facial.jpg",
                    Description = "Liệu trình Chăm Sóc Da Mặt tại LoanSpa được thiết kế riêng cho từng loại da.",
                    Price = 650000,
                    Duration = "90 phút",
                    Benefits = new List<string>
                    {
                        "Làm sạch sâu và loại bỏ tế bào chết",
                        "Cấp ẩm và nuôi dưỡng da",
                        "Cải thiện kết cấu và độ đàn hồi",
                        "Giảm nếp nhăn và dấu hiệu lão hóa",
                        "Làm sáng da và đều màu da"
                    },
                    Process = new List<string>
                    {
                        "Tư vấn và phân tích tình trạng da",
                        "Làm sạch và tẩy trang",
                        "Tẩy tế bào chết và hút mụn (nếu cần)",
                        "Massage mặt thư giãn",
                        "Đắp mặt nạ đặc trị",
                        "Dưỡng ẩm và chống nắng"
                    },
                    Note = "Sau khi trải nghiệm dịch vụ, bạn nên hạn chế ra nắng và sử dụng kem chống nắng."
                }
            }
            // Có thể thêm các dịch vụ khác từ View của bạn
        };

        // GET: api/Services
        [HttpGet]
        public ActionResult<IEnumerable<Service>> GetServices()
        {
            return Ok(_services.Values);
        }

        // GET: api/Services/massage
        [HttpGet("{id}")]
        public ActionResult<Service> GetService(string id)
        {
            if (!_services.ContainsKey(id))
            {
                return NotFound();
            }

            return _services[id];
        }
    }
}
