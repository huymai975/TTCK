namespace WebAppBookingBoat.Models.ViewModels
{
    public class HomeViewModel
    {
        // Danh sách các lịch trình sắp khởi hành
        public List<LichTrinh> LichTrinhs { get; set; } = new List<LichTrinh>();

        // Danh sách các tuyến đường để hiển thị trong phần Destination hoặc Select box
        public List<TuyenDuong> TuyenDuongs { get; set; } = new List<TuyenDuong>();
    }
}
