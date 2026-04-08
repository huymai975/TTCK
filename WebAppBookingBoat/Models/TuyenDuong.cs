using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("TuyenDuong")]
    public class TuyenDuong
    {
        [Key]
        [Display(Name = "Mã tuyến")]
        public int MaTuyen { get; set; }

        [Required(ErrorMessage = "Tên tuyến không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tên tuyến")]
        public string TenTuyen { get; set; } = default!; // Ví dụ: Sài Gòn - Vũng Tàu

        [Required(ErrorMessage = "Điểm đi không được để trống")]
        [StringLength(100)]
        [Display(Name = "Điểm đi")]
        // Kết hợp với DiemDen tạo thành Unique Index trong DbContext
        public string DiemDi { get; set; } = default!;

        [Required(ErrorMessage = "Điểm đến không được để trống")]
        [StringLength(100)]
        [Display(Name = "Điểm đến")]
        public string DiemDen { get; set; } = default!;

        [Range(0, 10000, ErrorMessage = "Khoảng cách phải là số dương")]
        [Display(Name = "Khoảng cách (km)")]
        public double KhoangCach { get; set; }

        [Required(ErrorMessage = "Thời gian dự kiến không được để trống")]
        [Display(Name = "Thời gian dự kiến")]
        public TimeSpan ThoiGianDuKien { get; set; } // Ví dụ: 02:30:00

        // --- Navigation Properties ---

        // Khởi tạo List để tránh lỗi NullReferenceException khi truy cập danh sách lịch trình
        public virtual ICollection<LichTrinh> LichTrinhs { get; set; } = new List<LichTrinh>();
    }
}