using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBookingBoat.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [Display(Name = "Mã tài khoản")]
        public int MaTK { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = default!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = default!;

        [Required]
        [StringLength(20)]
        [Display(Name = "Vai trò")]
        // Phải khớp chính xác với CheckConstraint: N'Admin', N'Nhân viên', N'Khách hàng'
        public string VaiTro { get; set; } = "Khách hàng";

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // --- Mối quan hệ (Navigation Properties) ---

        // Dùng dấu ? vì quan hệ là 0..1 (Tài khoản có thể không phải là Nhân viên)
        public virtual NhanVien? NhanVien { get; set; }

        public virtual KhachHang? KhachHang { get; set; }

        // Khởi tạo List để tránh lỗi Null khi truy cập .Logs
        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}