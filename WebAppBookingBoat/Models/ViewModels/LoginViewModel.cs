using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        public string? TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }
    }
}
