namespace WebAppBookingBoat.Models.ViewModels
{
    public class SearchViewModel
    {
        public TuyenDuong? TuyenDuong { get; set; }

        public DateTime? NgayGioKhoiHanh { get; set; }

        public List<LichTrinh> KetQuaLichTrinh { get; set; } = new List<LichTrinh>();
    }
}
