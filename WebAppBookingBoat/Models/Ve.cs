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
        public int? MaHoaDon { get; set; }

        [ForeignKey("MaHoaDon")]
        public virtual HoaDon? HoaDon { get; set; }

        [Required]
        [Display(Name = "Mã lịch trình")]
        public int MaLichTrinh { get; set; }

        [ForeignKey("MaLichTrinh")]
        public virtual LichTrinh? LichTrinh { get; set; }

        [Required(ErrorMessage = "Giá vé không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vé không được âm")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá vé thực tế")]
        public decimal GiaVe { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hợp lệ"; // Ví dụ: Hợp lệ, Đã sử dụng, Đã hủy


        // --- LIÊN KẾT VỚI GHẾ ---
        [Required(ErrorMessage = "Vui lòng chọn chỗ ngồi")]
        [Display(Name = "Mã ghế")]
        public int MaGhe { get; set; }

        [ForeignKey("MaGhe")]
        public virtual Ghe? Ghe { get; set; }

        // --- Navigation Property ---

        // Một vé chỉ có tối đa một đánh giá (1-1 hoặc 1-0)
        public virtual DanhGia? DanhGia { get; set; }
    }
}