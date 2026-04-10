using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class TuyenDuongViewModel
    {
        // Fix lỗi thiếu MaTuyen
        public int MaTuyen { get; set; }

        [Required(ErrorMessage = "Tên tuyến không được để trống")]
        public string TenTuyen { get; set; } = default!;

        [Required(ErrorMessage = "Điểm đi không được để trống")]
        public string DiemDi { get; set; } = default!;

        [Required(ErrorMessage = "Điểm đến không được để trống")]
        public string DiemDen { get; set; } = default!;

        public double KhoangCach { get; set; }

        [Required(ErrorMessage = "Thời gian dự kiến không được để trống")]
        public TimeSpan ThoiGianDuKien { get; set; }

        // Fix lỗi thiếu HinhAnhCu (Dùng cho Edit)
        public string? HinhAnhCu { get; set; }

        // Fix lỗi thiếu ImageFile (Sử dụng tên ImageFile cho giống trang Tàu)
        public IFormFile? ImageFile { get; set; }

        // Thuộc tính để nhận HinhAnhFile nếu View vẫn đang dùng tên đó
        public IFormFile? HinhAnhFile { get => ImageFile; set => ImageFile = value; }
    }
}