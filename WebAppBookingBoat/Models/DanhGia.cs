using System;
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

        // --- Khóa ngoại nối với bảng Vé ---
        [Required(ErrorMessage = "Đánh giá phải thuộc về một mã vé cụ thể")]
        [Display(Name = "Mã vé")]
        // Đã được siết Unique Index trong DbContext (Quan hệ 1-1)
        public int MaVe { get; set; }

        [ForeignKey("MaVe")]
        public virtual Ve Ve { get; set; } = default!;

        [Required(ErrorMessage = "Vui lòng chọn số sao")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        [Display(Name = "Số sao")]
        // DB siết: CK_DG_SoSao (1-5)
        public int SoSao { get; set; }

        [StringLength(1000, ErrorMessage = "Nội dung phản hồi không quá 1000 ký tự")]
        [Display(Name = "Nội dung phản hồi")]
        public string? NoiDung { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái hiển thị")]
        // Phải khớp chính xác: N'Chờ duyệt', N'Đã hiển thị', N'Đã ẩn'
        public string TrangThai { get; set; } = "Chờ duyệt";
    }
}