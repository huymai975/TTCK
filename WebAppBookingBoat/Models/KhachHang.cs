using System;
using System.Collections.Generic;
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

        // --- Khóa ngoại trỏ về bảng TaiKhoan (0..1) ---
        [Display(Name = "Mã tài khoản")]
        public int? MaTK { get; set; }

        [ForeignKey("MaTK")]
        // Dùng dấu ? vì khách vãng lai sẽ không có TaiKhoan
        public virtual TaiKhoan? TaiKhoan { get; set; }

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
        // DB siết: CK_KH_Sdt_Format (>=10 số, chỉ chứa số)
        public string Sdt { get; set; } = default!;

        [Required(ErrorMessage = "Email không được để trống")] // Thêm nếu bắt buộc
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        // DB siết: CK_KH_Email_Format (_@_._)
        public string Email { get; set; } = default!;

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; } // Để ? nếu địa chỉ không bắt buộc

        // --- Mối quan hệ ---

        // Khởi tạo sẵn List để tránh lỗi NullReferenceException
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

        public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
    }
}