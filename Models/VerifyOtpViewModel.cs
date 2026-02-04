using System.ComponentModel.DataAnnotations;

namespace QuanLyRuiRoTinDung.Models
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã xác nhận")]
        [Display(Name = "Mã xác nhận")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nhận phải có 6 chữ số")]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
