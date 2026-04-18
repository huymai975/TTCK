namespace WebAppBookingBoat.Models.ViewModels
{
    public class KhuyenMaiViewModel
    {
        public string MaKM { get; set; } = default!;
        public string TenChuongTrinh { get; set; } = default!;
        public string? HinhAnh { get; set; }
        public string? MoTa { get; set; }
        public double PhanTramGiam { get; set; }
        public decimal SoTienToiDaGiam { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoLuotDaDung { get; set; }
        public int SoNgayConLai => (NgayKetThuc - DateTime.Now).Days;
    }
}