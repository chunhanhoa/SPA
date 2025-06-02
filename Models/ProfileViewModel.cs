using System;
using System.ComponentModel.DataAnnotations;

namespace QL_Spa.Models
{
    public class ProfileViewModel
    {
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại hệ thống")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại liên hệ")]
        public string Phone { get; set; }

        [Display(Name = "Ngày tham gia")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Tổng chi tiêu")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }
    }
}
