using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        [Display(Name = "Mã hóa đơn")]
        public int MaHoaDon { get; set; }

        [Required]
        [Display(Name = "Mã khách hàng")]
        public int MaKH { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang KhachHang { get; set; } = default!;

        [Display(Name = "Mã nhân viên")]
        public int? MaNV { get; set; } // Nullable nếu khách tự mua online

        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã khuyến mãi")]
        public string? MaKM { get; set; } // Nullable nếu không dùng khuyến mãi

        [ForeignKey("MaKM")]
        public virtual KhuyenMai? KhuyenMai { get; set; }

        [Display(Name = "Ngày lập")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng vé phải từ 1 đến 100")]
        [Display(Name = "Số lượng vé")]
        public int SoLuongVe { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tạm tính")]
        public decimal TamTinh { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền giảm")]
        public decimal SoTienGiam { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Phương thức TT")]
        public string PhuongThucTT { get; set; } = "Tiền mặt"; // Ví dụ: Tiền mặt, Chuyển khoản, VNPay

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chưa thanh toán"; // Ví dụ: Đã thanh toán, Chưa thanh toán, Đã hủy

        // --- Navigation Property ---
        public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
    }
}