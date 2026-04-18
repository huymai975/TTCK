namespace WebAppBookingBoat.ViewModels
{
    public class PassengerListViewModel
    {
        public int MaLichTrinh { get; set; }
        public string? TenTau { get; set; }
        public string? TuyenDuong { get; set; }
        public DateTime NgayKhoiHanh { get; set; }

        // Danh sách chi tiết từng hành khách
        public List<PassengerItem> Passengers { get; set; } = new List<PassengerItem>();
    }

    public class PassengerItem
    {
        public int MaVe { get; set; }
        public string? TenHanhKhach { get; set; } // Lấy từ HoTen của KhachHang
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? TenGhe { get; set; }
        public string? LoaiGhe { get; set; }
        public string? TrangThaiVe { get; set; }
        public decimal GiaVe { get; set; }
    }
}