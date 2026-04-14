using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("Logs")] // Thường dùng số nhiều cho tên bảng
    public class Log
    {
        [Key]
        [Display(Name = "Mã nhật ký")]
        public int MaLog { get; set; }

        // Khóa ngoại trỏ về Identity (AspNetUsers)
        [Display(Name = "Mã tài khoản")]
        public string? MaTK { get; set; }

        [ForeignKey("MaTK")]
        public virtual AppUser? AppUser { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại")]
        // Ví dụ: Info, Warning, Error, Critical (Dễ phân loại khi xem)
        public string LoaiLog { get; set; } = "Info";

        [Required]
        [StringLength(100)]
        [Display(Name = "Hành động")]
        // Ví dụ: N'Đăng nhập', N'Xóa mềm Khuyến mãi'
        public string HanhDong { get; set; } = default!;

        [Required]
        [StringLength(100)]
        [Display(Name = "Bảng tác động")]
        public string BangTacDong { get; set; } = default!;

        [Display(Name = "Nội dung chi tiết")]
        public string? NoiDungChiTiet { get; set; }

        [StringLength(50)]
        [Display(Name = "Địa chỉ IP")]
        public string? IpAddress { get; set; }

        [Required]
        [Display(Name = "Thời gian")]
        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}