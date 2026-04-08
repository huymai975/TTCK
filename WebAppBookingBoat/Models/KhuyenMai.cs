using System;
using System.Collections.Generic;
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
        // Ví dụ: SUMMER2026, KM50...
        public string MaKM { get; set; } = default!;

        [Required(ErrorMessage = "Tên chương trình không được để trống")]
        [StringLength(255)]
        [Display(Name = "Tên chương trình")]
        public string TenChuongTrinh { get; set; } = default!;

        [Required]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        [Display(Name = "Phần trăm giảm (%)")]
        // DB siết: CK_KM_PhanTram
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
        // DB siết: CK_KM_ThoiGian ([NgayKetThuc] > [NgayBatDau])
        public DateTime NgayKetThuc { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // --- Navigation Property ---

        // Khởi tạo List để tránh lỗi khi truy vấn danh sách hóa đơn đã áp dụng mã này
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}