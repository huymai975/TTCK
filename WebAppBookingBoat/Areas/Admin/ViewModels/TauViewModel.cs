using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class TauViewModel
    {
        // ID chỉ dùng khi Edit, khi Create sẽ mang giá trị 0
        public int MaTau { get; set; }

        [Required(ErrorMessage = "Tên tàu không được để trống")]
        [Display(Name = "Tên tàu")]
        public string TenTau { get; set; }

        // Dùng để nhận file từ Form (Create bắt buộc, Edit không)
        [Display(Name = "Hình ảnh tàu")]
        public IFormFile? ImageFile { get; set; }

        // Dùng để chứa tên file cũ khi Edit
        public string? HinhAnhCu { get; set; }

        [Required(ErrorMessage = "Số ghế thường không được trống")]
        [Range(0, 1000, ErrorMessage = "Số ghế từ 0 - 1000")]
        [Display(Name = "Số ghế thường")]
        public int SoGheThuong { get; set; }

        [Required(ErrorMessage = "Số ghế VIP không được trống")]
        [Range(0, 500, ErrorMessage = "Số ghế từ 0 - 500")]
        [Display(Name = "Số ghế VIP")]
        public int SoGheVIP { get; set; }

        [Display(Name = "Tổng số ghế")]
        public int TongSoGhe { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true;
    }
}