using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("Log")]
    public class Log
    {
        [Key]
        [Display(Name = "Mã nhật ký")]
        public int MaLog { get; set; }

        // --- Khóa ngoại nối với bảng Tài khoản (Người thực hiện) ---
        [Required]
        // --- Khóa ngoại trỏ về Identity (AspNetUsers) ---
        // Thêm dấu ? để cho phép NULL (dành cho khách vãng lai)
        [Display(Name = "Mã tài khoản")]
        public string? MaTK { get; set; }
        // Cho phép null để EF không bắt buộc phải có tài khoản khi lưu
        [ForeignKey("MaTK")]
        public virtual AppUser? AppUser { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Hành động")]
        // Ví dụ: N'Đăng nhập', N'Cập nhật giá vé', N'Xóa hóa đơn'
        public string HanhDong { get; set; } = default!;

        [Required]
        [StringLength(100)]
        [Display(Name = "Bảng tác động")]
        // Ví dụ: 'HoaDon', 'LichTrinh'
        public string BangTacDong { get; set; } = default!;

        [Display(Name = "Nội dung chi tiết")]
        // Không giới hạn độ dài hoặc để rất dài (Max) để lưu vết đầy đủ
        public string? NoiDungChiTiet { get; set; }

        [Required]
        [Display(Name = "Thời gian thực hiện")]
        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}