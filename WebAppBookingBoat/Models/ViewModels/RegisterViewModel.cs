using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên")]
        public string? HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại phải từ 10-11 số")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        public string? TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu không khớp")]
        [DataType(DataType.Password)]
        public string? NhapLaiMatKhau { get; set; }
    }
}
