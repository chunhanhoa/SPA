using System.ComponentModel.DataAnnotations;

namespace LoanSpa.Models
{
    public class EditProfileViewModel
    {
        public int CustomerId { get; set; }
        
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(50, ErrorMessage = "Họ và tên không quá 50 ký tự")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;
    }
}
