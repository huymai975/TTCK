using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        [Display(Name = "Mã khách hàng")]
        public int MaKH { get; set; }

        // --- Khóa ngoại trỏ về Identity (AspNetUsers) ---
        // Thêm dấu ? để cho phép NULL (dành cho khách vãng lai)
        [Display(Name = "Mã tài khoản")]
        public string? MaTK { get; set; }
        // Cho phép null để EF không bắt buộc phải có tài khoản khi lưu
        [ForeignKey("MaTK")]
        public virtual AppUser? AppUser { get; set; }

        // --- Thông tin cá nhân ---
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = default!;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string Sdt { get; set; } = default!;

        // Đối với khách vãng lai, Email là thông tin liên lạc quan trọng thay cho tài khoản
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = default!;

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // --- Mối quan hệ ---
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}