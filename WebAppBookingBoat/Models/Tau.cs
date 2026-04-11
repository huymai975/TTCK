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

        [StringLength(255)]
        [Display(Name = "Hình ảnh tàu")]
        public string? HinhAnh { get; set; } // Lưu tên file: tau-01.jpg

        [NotMapped]
        [Display(Name = "Tải ảnh lên")]
        public IFormFile? ImageFile { get; set; } // Phục vụ lúc Upload file trong Controller

        [Required]
        [Range(1, 1000, ErrorMessage = "Tổng số ghế phải từ 1 đến 1000")]
        [Display(Name = "Tổng số ghế")]
        public int TongSoGhe { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true: Đang hoạt động, false: Bảo trì

        // --- Navigation Properties ---

        // Khởi tạo List để tránh lỗi NullReferenceException khi truy cập danh sách ghế
        public virtual ICollection<Ghe> Ghes { get; set; } = new List<Ghe>();

        // Một tàu có thể xuất hiện trong nhiều lịch trình chạy
        public virtual ICollection<LichTrinh> LichTrinhs { get; set; } = new List<LichTrinh>();
    }
}