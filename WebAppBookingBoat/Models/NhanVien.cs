using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        [Display(Name = "Mã nhân viên")]
        public int MaNV { get; set; }

        [Display(Name = "Mã tài khoản")]
        // Tên cột trong bảng KhachHang của bạn
        public string? MaTK { get; set; }

        // Khai báo thuộc tính dẫn hướng trỏ tới lớp AppUser
        [ForeignKey("MaTK")]
        public virtual AppUser? AppUser { get; set; }

        // --- Thông tin cá nhân ---
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = default!;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        // DB siết: CK_NV_Sdt_Format (>=10 số và chỉ chứa số)
        public string Sdt { get; set; } = default!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        // DB siết: CK_NV_Email_Format (định dạng _@_._)
        public string Email { get; set; } = default!;

        [StringLength(20)]
        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; } // Ví dụ: Quản lý, Nhân viên bán vé

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương không được là số âm")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Lương")]
        public decimal Luong { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // --- Mối quan hệ ---

        // Một nhân viên có thể lập nhiều hóa đơn
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}