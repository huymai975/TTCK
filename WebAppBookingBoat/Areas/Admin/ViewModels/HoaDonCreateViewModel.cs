using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class HoaDonCreateViewModel
    {
        // 1. Phân loại khách hàng
        public bool IsVangLai { get; set; } // Để bind từ checkbox chuyển đổi

        [Display(Name = "Khách hàng hệ thống")]
        // Bỏ Required ở đây vì nếu là khách vãng lai thì MaKH sẽ null
        public int? MaKH { get; set; }

        // 2. Thông tin khách vãng lai
        [Display(Name = "Họ tên khách vãng lai")]
        [StringLength(100, ErrorMessage = "Tên không quá 100 ký tự")]
        public string? TenKhachVangLai { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])([0-9]{8})$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string? SdtKhachVangLai { get; set; }

        [Display(Name = "Email khách hàng")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? EmailKhachVangLai { get; set; }

        // 3. Thông tin hóa đơn
        [Display(Name = "Nhân viên lập")]
        public int? MaNV { get; set; }

        [Display(Name = "Mã khuyến mãi")]
        public string? MaKM { get; set; }

        [Display(Name = "Ngày lập")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucTT { get; set; } = "Tiền mặt";

        [Required]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chưa thanh toán";

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Display(Name = "Lịch trình")]
        [Required(ErrorMessage = "Vui lòng chọn chuyến đi")]
        public int MaLichTrinh { get; set; }

        // 4. Danh sách vé (Ghế)
        [Display(Name = "Danh sách vé chọn")]
        public List<int> SelectedVeIds { get; set; } = new List<int>();

        // 5. Tài chính
        [Display(Name = "Tạm tính")]
        public decimal TamTinh { get; set; }

        [Display(Name = "Số tiền giảm")]
        public decimal SoTienGiam { get; set; } = 0;

        [Display(Name = "Tổng tiền")]
        [Range(0, double.MaxValue)]
        public decimal TongTien { get; set; }

        public int SoLuongVe => SelectedVeIds?.Count ?? 0;

        // 6. Dữ liệu đổ lên Dropdown
        public SelectList? DanhSachKhachHang { get; set; }
        public SelectList? DanhSachKhuyenMai { get; set; }
        public SelectList? DanhSachLichTrinh { get; set; }
    }
}