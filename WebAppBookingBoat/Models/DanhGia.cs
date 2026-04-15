using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thêm thư viện này
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

        [Required(ErrorMessage = "Đánh giá phải thuộc về một hóa đơn cụ thể")]
        [Display(Name = "Mã hóa đơn")]
        public int MaHoaDon { get; set; }

        [ForeignKey("MaHoaDon")]
        [ValidateNever]
        public virtual HoaDon? HoaDon { get; set; }

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
        [ValidateNever]
        public string? HinhAnh { get; set; }

        [Display(Name = "Ngày đánh giá")]
        [ValidateNever]
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        [StringLength(1000, ErrorMessage = "Nội dung phản hồi không quá 1000 ký tự")]
        [Display(Name = "Phản hồi của Admin")]
        [DataType(DataType.MultilineText)]
        [ValidateNever]
        public string? PhanHoiAdmin { get; set; }

        [Display(Name = "Ngày phản hồi")]
        [ValidateNever]
        public DateTime? NgayPhanHoi { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái hiển thị")]
        [ValidateNever]
        public string TrangThai { get; set; } = "Chờ duyệt";
    }
}