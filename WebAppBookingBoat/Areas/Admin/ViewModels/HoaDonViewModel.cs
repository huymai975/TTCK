namespace WebAppBookingBoat.ViewModels
{
    public class HoaDonViewModel
    {
        public int MaHoaDon { get; set; }

        // --- Thông tin Khách hàng & Liên lạc ---
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoaiKH { get; set; } = string.Empty;

        // --- Thông tin nhân viên & Thời gian ---
        public string? TenNhanVien { get; set; }
        public DateTime NgayLap { get; set; }

        // --- Tài chính & Định dạng ---
        public int SoLuongVe { get; set; }
        public decimal TamTinh { get; set; }
        public decimal SoTienGiam { get; set; }
        public decimal TongTien { get; set; }

        // Thuộc tính hỗ trợ hiển thị nhanh trên View
        public string TongTienFormat => TongTien.ToString("N0") + "đ";

        // --- Trạng thái & Ghi chú (Mới cập nhật) ---
        public string PhuongThucTT { get; set; } = "Tiền mặt";
        public string TrangThai { get; set; } = "Chưa thanh toán";
        public string? GhiChu { get; set; } // Nhận từ thuộc tính mới của Model

        // --- Danh sách vé con (Cho trang Details) ---
        public List<VeChiTietViewModel> DanhSachVe { get; set; } = new List<VeChiTietViewModel>();
    }

    public class VeChiTietViewModel
    {
        public int MaVe { get; set; }
        public string TenGhe { get; set; } = string.Empty;
        public string LoaiGhe { get; set; } = string.Empty;
        public decimal GiaVe { get; set; }
    }
}