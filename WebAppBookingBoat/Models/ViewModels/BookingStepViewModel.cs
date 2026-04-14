namespace WebAppBookingBoat.Models.ViewModels
{
    public class BookingStepViewModel
    {
        // 1. Thông tin định danh lịch trình
        public int MaLichTrinh { get; set; }

        // 2. Dữ liệu hiển thị (Dùng để render thông tin chuyến đi & sơ đồ ghế)
        public LichTrinh? LichTrinh { get; set; }

        // 3. Danh sách ID ghế đã có người đặt (để làm mờ/khóa ghế trên giao diện)
        public List<int> GheDaBanIds { get; set; } = new List<int>();

        // 4. Giá vé cơ bản (lấy từ LichTrinh để hiển thị/tính toán nhanh ở Client bằng JS)
        public decimal GiaVeCoBan { get; set; }

        // 5. Dữ liệu nhận về từ Form (Khi người dùng click chọn ghế trên sơ đồ)
        // Đây là danh sách các ID ghế mà khách hàng đã chọn
        public List<int> SelectedGheIds { get; set; } = new List<int>();

        // 6. Thông tin bổ sung (tùy chọn)
        // Nếu bạn muốn cho phép khách hàng nhập mã giảm giá ngay tại bước chọn ghế

        public string? MaKM { get; set; }

        // Các thuộc tính hỗ trợ tính toán nhanh hiển thị trên UI
        public decimal TiLePhiVIP { get; set; } = 1.2m; // Tương ứng 20% như logic Admin
    }
}