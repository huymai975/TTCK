using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("KhuyenMai")]
    public class KhuyenMai
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Mã khuyến mãi")]
        public string MaKM { get; set; } = default!;

        [Required(ErrorMessage = "Tên chương trình không được để trống")]
        [StringLength(255)]
        [Display(Name = "Tên chương trình")]
        public string TenChuongTrinh { get; set; } = default!;

        // --- PHẦN BỔ SUNG MỚI ---
        [StringLength(500)]
        [Display(Name = "Hình ảnh")]
        // Lưu tên file ảnh (ví dụ: summer-sale.jpg)
        public string? HinhAnh { get; set; }

        [StringLength(1000)]
        [Display(Name = "Mô tả ngắn")]
        [DataType(DataType.MultilineText)] // Hỗ trợ hiển thị textarea trong View
        public string? MoTa { get; set; }
        // ------------------------

        [Required]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        [Display(Name = "Phần trăm giảm (%)")]
        public double PhanTramGiam { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa không được âm")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền giảm tối đa")]
        public decimal SoTienToiDaGiam { get; set; }

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; } = DateTime.Now.AddDays(7);

        [Required]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chưa diễn ra";

        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}