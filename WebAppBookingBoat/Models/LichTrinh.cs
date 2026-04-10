using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("LichTrinh")]
    public class LichTrinh
    {
        [Key]
        [Display(Name = "Mã lịch trình")]
        public int MaLichTrinh { get; set; }

        // --- Khóa ngoại nối với Tuyến đường ---
        [Required(ErrorMessage = "Vui lòng chọn tuyến đường")]
        [Display(Name = "Tuyến đường")]
        public int MaTuyen { get; set; }

        [ForeignKey("MaTuyen")]
        public virtual TuyenDuong TuyenDuong { get; set; } = default!;

        // --- Khóa ngoại nối với Tàu ---
        [Required(ErrorMessage = "Vui lòng chọn tàu")]
        [Display(Name = "Tàu")]
        public int MaTau { get; set; }

        [ForeignKey("MaTau")]
        public virtual Tau Tau { get; set; } = default!;

        [Required(ErrorMessage = "Giờ khởi hành không được để trống")]
        [Display(Name = "Giờ khởi hành")]
        public DateTime NgayGioKhoiHanh { get; set; }

        [Required(ErrorMessage = "Giờ cập bến dự kiến không được để trống")]
        [Display(Name = "Giờ cập bến dự kiến")]
        // DB siết: CK_LT_ThoiGian ([NgayGioCapBenDuKien] > [NgayGioKhoiHanh])
        public DateTime NgayGioCapBenDuKien { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vé không được là số âm")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá vé cơ bản")]
        public decimal GiaVeCoBan { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Số ghế trống phải từ 0 trở lên")]
        [Display(Name = "Số ghế trống")]
        public int SoGheTrong { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Sắp khởi hành";

        // --- Navigation Properties ---

        // Khởi tạo List để tránh lỗi Null
        public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();

        // Nếu bạn giữ lại ICollection này (mặc dù đã bỏ MaLT bên bảng DanhGia thì EF vẫn tự map được qua quan hệ trung gian nếu cần)
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}