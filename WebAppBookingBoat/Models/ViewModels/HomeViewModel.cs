namespace WebAppBookingBoat.Models.ViewModels
{
    public class HomeViewModel
    {
        // Danh sách các lịch trình sắp khởi hành
        public IEnumerable<LichTrinh> LichTrinhs { get; set; } = new List<LichTrinh>();

        // Danh sách các tuyến đường để hiển thị trong phần Destination hoặc Select box
        public IEnumerable<TuyenDuong> TuyenDuongs { get; set; } = new List<TuyenDuong>();

        public IEnumerable<DanhGia>? DanhGias { get; set; }
    }
}
