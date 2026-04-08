using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppBookingBoat.Models;

namespace WebAppBookingBoat.Models
{
    [Table("Ghe")]
    public class Ghe
    {
        [Key]
        [Display(Name = "Mã ghế")]
        public int MaGhe { get; set; }

        // --- Khóa ngoại trỏ về bảng Tau ---
        [Required]
        public int MaTau { get; set; }

        [ForeignKey("MaTau")]
        public virtual Tau Tau { get; set; } = default!;

        [Required(ErrorMessage = "Tên ghế không được để trống")]
        [StringLength(10)]
        [Display(Name = "Tên ghế")]
        public string TenGhe { get; set; } = default!;// Ví dụ: A01, B12

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại ghế")]
        public string LoaiGhe { get; set; } = "Thường";// Thường, VIP

        // --- Navigation Properties ---

        // Trong sơ đồ tinh gọn của bạn, Vé sẽ không nối trực tiếp với Ghế 
        // nhưng nếu cần quản lý trạng thái ghế theo lịch trình, ta sẽ xử lý ở tầng Logic.
    }
}