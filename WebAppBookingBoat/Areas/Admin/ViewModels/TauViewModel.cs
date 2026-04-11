using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class TauViewModel
    {
        public int MaTau { get; set; }

        [Required(ErrorMessage = "Tên tàu không được để trống")]
        [Display(Name = "Tên tàu")]
        public string TenTau { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh tàu")]
        public IFormFile? ImageFile { get; set; }

        // Dùng để chứa tên file cũ khi Edit để hiển thị lại ảnh nếu không thay đổi
        public string? HinhAnhCu { get; set; }

        [Required(ErrorMessage = "Tổng số ghế không được để trống")]
        [Display(Name = "Tổng số ghế")]
        public int TongSoGhe { get; set; } = 0;


        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true;
    }
}