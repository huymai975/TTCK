using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        [Display(Name = "Mã đánh giá")]
        public int MaDanhGia { get; set; }

        // --- Liên kết 1-1 với Hóa đơn ---
        [Required(ErrorMessage = "Đánh giá phải thuộc về một hóa đơn cụ thể")]
        [Display(Name = "Mã hóa đơn")]
        public int MaHoaDon { get; set; }

        [ForeignKey("MaHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = default!;

        // --- Nội dung từ Khách hàng ---
        [Required(ErrorMessage = "Vui lòng chọn số sao")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; }

        [StringLength(1000, ErrorMessage = "Nội dung phản hồi không quá 1000 ký tự")]
        [Display(Name = "Nội dung khách hàng")]
        [DataType(DataType.MultilineText)]
        public string? NoiDung { get; set; }

        [StringLength(255)]
        [Display(Name = "Hình ảnh thực tế")]
        public string? HinhAnh { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        // --- PHẦN BỔ SUNG: PHẢN HỒI TỪ ADMIN ---
        [StringLength(1000, ErrorMessage = "Nội dung phản hồi không quá 1000 ký tự")]
        [Display(Name = "Phản hồi của Admin")]
        [DataType(DataType.MultilineText)]
        public string? PhanHoiAdmin { get; set; }

        [Display(Name = "Ngày phản hồi")]
        public DateTime? NgayPhanHoi { get; set; }
        // ---------------------------------------

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái hiển thị")]
        // Mặc định: N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn'
        public string TrangThai { get; set; } = "Chờ duyệt";
    }
}