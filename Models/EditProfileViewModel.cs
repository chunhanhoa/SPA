using System.ComponentModel.DataAnnotations;

namespace QL_Spa.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá {1} ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá {1} ký tự.")]
        public string Phone { get; set; }
    }
}
