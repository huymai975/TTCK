using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.Models.ViewModels
{
    public class ProfileVM
    {
        // --- Thông tin từ Model KhachHang ---
        public int MaKH { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Display(Name = "Số điện thoại")]
        public string Sdt { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        // --- Thông tin tài khoản ---
        public string? TenDangNhap { get; set; }

        // --- Thông tin đổi mật khẩu (Dùng chung cho form bên cạnh) ---
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string? OldPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [Display(Name = "Mật khẩu mới")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string? ConfirmPassword { get; set; }
    }
}