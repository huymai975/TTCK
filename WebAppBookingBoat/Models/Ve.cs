using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("Ve")]
    public class Ve
    {
        [Key]
        [Display(Name = "Mã vé")]
        public int MaVe { get; set; }

        [Required]
        [Display(Name = "Mã hóa đơn")]
        public int MaHoaDon { get; set; }

        [ForeignKey("MaHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = default!;

        [Required]
        [Display(Name = "Mã lịch trình")]
        public int MaLichTrinh { get; set; }

        [ForeignKey("MaLichTrinh")]
        public virtual LichTrinh LichTrinh { get; set; } = default!;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vé không được âm")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá vé")]
        public decimal GiaVe { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hợp lệ"; // Ví dụ: Hợp lệ, Đã sử dụng, Đã hủy

        // --- Navigation Property ---

        // Dùng ? vì quan hệ 1-1: Vé có thể chưa có đánh giá
        public virtual DanhGia? DanhGia { get; set; }
    }
}