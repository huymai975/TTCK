using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class HoaDonCreateViewModel
    {
        [Display(Name = "Khách hàng")]
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int MaKH { get; set; }

        [Display(Name = "Nhân viên lập")]
        public int? MaNV { get; set; }

        [Display(Name = "Mã khuyến mãi")]
        public string? MaKM { get; set; }

        [Display(Name = "Ngày lập")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucTT { get; set; } = "Tiền mặt";

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chưa thanh toán";

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Display(Name = "Lịch trình")]
        [Required(ErrorMessage = "Vui lòng chọn lịch trình")]
        public int MaLichTrinh { get; set; }

        // Danh sách ID vé được chọn từ Checkbox
        // Sửa: Đảm bảo không bị null khi bind dữ liệu từ Form gửi lên
        [Display(Name = "Danh sách vé chọn")]
        public List<int> SelectedVeIds { get; set; } = new List<int>();

        [Display(Name = "Tạm tính")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tạm tính phải lớn hơn 0")]
        public decimal TamTinh { get; set; }

        [Display(Name = "Số tiền giảm")]
        public decimal SoTienGiam { get; set; } = 0;

        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        // Bổ sung: Thuộc tính này để View dễ dàng hiển thị số lượng vé mà không cần đếm mảng JS
        [Display(Name = "Số lượng vé")]
        public int SoLuongVe => SelectedVeIds?.Count ?? 0;

        // SelectLists khởi tạo rỗng để tránh NullReference ở View
        public SelectList? DanhSachKhachHang { get; set; }
        public SelectList? DanhSachKhuyenMai { get; set; }
        public SelectList? DanhSachLichTrinh { get; set; }
    }
}