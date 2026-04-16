using WebAppBookingBoat.Models;

namespace WebAppBookingBoat.ViewModels
{
    public class nvDashboardViewModel
    {

        // Thuộc tính hỗ trợ bộ lọc (Quan trọng cho giao diện)
        public string CurrentFilter { get; set; } = "today";

        // Nhóm chỉ số tổng quát (4 boxes trên cùng)
        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public int SoKhachHang { get; set; }
        public int SoTauDangChay { get; set; }

        // Nhóm chi tiết Hóa đơn & Trạng thái
        public int TongSoHoaDon { get; set; }
        public int HoaDonMoiTrongNgay { get; set; }
        public int HoaDonChoXuLy { get; set; }
        public int HoaDonQuaHan { get; set; }

        // Hiệu suất & Tăng trưởng
        public double TyLeLapDay { get; set; } // Tính theo %
        public decimal PhanTramTangTruong { get; set; }
        public string? XuHuongTangTruong { get; set; } // "up" hoặc "down"

        // Nhóm danh sách hiển thị (Tables/Lists)
        public List<Log> LogGanDay { get; set; } = new List<Log>();
        public List<HoaDon> HoaDonMoiNhat { get; set; } = new List<HoaDon>();

        // Dữ liệu cho biểu đồ Chart.js
        public List<decimal> DoanhThu7Ngay { get; set; } = new List<decimal>();
        public List<string> Labels7Ngay { get; set; } = new List<string>();
    }
}