namespace WebAppBookingBoat.Models.ViewModels
{
    public class DanhGiaViewModel
    {
        // --- Thông tin chính của Đánh giá ---
        public int MaDanhGia { get; set; }

        public int SoSao { get; set; }

        public string? NoiDung { get; set; }

        public string? HinhAnh { get; set; }

        public DateTime NgayDanhGia { get; set; }

        // --- Thông tin từ Admin ---
        public string? PhanHoiAdmin { get; set; }

        public DateTime? NgayPhanHoi { get; set; }

        // --- Thông tin liên kết (Join từ các bảng khác) ---

        // Từ HoaDon -> KhachHang -> HoTen
        public string TenKhachHang { get; set; } = "Khách hàng ẩn danh";

        // Từ HoaDon -> Ve -> LichTrinh -> TuyenDuong -> TenTuyenDuong
        public string? TenTuyenDuong { get; set; }

        // Mã hóa đơn để hiển thị số hiệu hoặc link chi tiết nếu cần
        public int MaHoaDon { get; set; }

        // Trạng thái hiển thị (để xử lý logic badge nếu cần)
        public string? TrangThai { get; set; }
    }
}