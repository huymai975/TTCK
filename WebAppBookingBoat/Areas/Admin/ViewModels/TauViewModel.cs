using Microsoft.AspNetCore.Http;
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

        [Required(ErrorMessage = "Số ghế thường không được trống")]
        [Range(0, 1000, ErrorMessage = "Số ghế thường phải từ 0 - 1000")]
        [Display(Name = "Số ghế thường")]
        public int SoGheThuong { get; set; }

        [Required(ErrorMessage = "Số ghế VIP không được trống")]
        [Range(0, 500, ErrorMessage = "Số ghế VIP phải từ 0 - 500")]
        [Display(Name = "Số ghế VIP")]
        public int SoGheVIP { get; set; }

        // Logic tự động tính tổng ghế để tránh sai sót dữ liệu
        [Display(Name = "Tổng số ghế")]
        public int TongSoGhe => SoGheThuong + SoGheVIP;

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true;
    }
}