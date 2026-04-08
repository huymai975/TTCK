using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("Tau")]
    public class Tau
    {
        [Key]
        [Display(Name = "Mã tàu")]
        public int MaTau { get; set; }

        [Required(ErrorMessage = "Tên tàu không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên tàu")]
        // Đã được siết Unique trong DbContext
        public string TenTau { get; set; } = default!;

        [Required]
        [Range(1, 1000, ErrorMessage = "Tổng số ghế phải từ 1 đến 1000")]
        [Display(Name = "Tổng số ghế")]
        public int TongSoGhe { get; set; }

        [Range(0, 1000, ErrorMessage = "Số ghế không được âm")]
        [Display(Name = "Số ghế thường")]
        public int SoGheThuong { get; set; }

        [Range(0, 1000, ErrorMessage = "Số ghế không được âm")]
        [Display(Name = "Số ghế VIP")]
        public int SoGheVIP { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true: Đang hoạt động, false: Bảo trì

        // --- Navigation Properties ---

        // Khởi tạo List để tránh lỗi NullReferenceException khi truy cập danh sách ghế
        public virtual ICollection<Ghe> Ghes { get; set; } = new List<Ghe>();

        // Một tàu có thể xuất hiện trong nhiều lịch trình chạy
        public virtual ICollection<LichTrinh> LichTrinhs { get; set; } = new List<LichTrinh>();
    }
}