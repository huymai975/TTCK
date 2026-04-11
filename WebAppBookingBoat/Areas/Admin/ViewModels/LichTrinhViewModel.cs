using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class LichTrinhViewModel
    {
        public int MaLichTrinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tuyến đường")]
        [Display(Name = "Tuyến đường")]
        public int MaTuyen { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tàu")]
        [Display(Name = "Tàu")]
        public int MaTau { get; set; }

        [Required(ErrorMessage = "Giờ khởi hành không được để trống")]
        [Display(Name = "Giờ khởi hành")]
        [DataType(DataType.DateTime)]
        public DateTime NgayGioKhoiHanh { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Giờ cập bến dự kiến không được để trống")]
        [Display(Name = "Giờ cập bến dự kiến")]
        [DataType(DataType.DateTime)]
        public DateTime NgayGioCapBenDuKien { get; set; } = DateTime.Now.AddHours(2);

        [Required(ErrorMessage = "Vui lòng nhập giá vé")]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá vé phải từ 1,000đ trở lên")]
        [Display(Name = "Giá vé cơ bản (VNĐ)")]
        public decimal GiaVeCoBan { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Sắp khởi hành";

        // --- BỔ SUNG CÁC TRƯỜNG HIỂN THỊ (FIX LỖI CS1061) ---
        public string? TenTuyen { get; set; }
        public string? TenTau { get; set; }
        public string? DiemDi { get; set; }     // Thêm để fix lỗi dòng 200
        public string? DiemDen { get; set; }    // Thêm để fix lỗi dòng 200
        public int TongSoGhe { get; set; }      // Thêm để fix lỗi dòng 214
        public int SoGheTrong { get; set; }

        // --- Danh sách SelectList để đổ vào Dropdown trong View ---
        public IEnumerable<SelectListItem>? DanhSachTuyen { get; set; }
        public IEnumerable<SelectListItem>? DanhSachTau { get; set; }
    }
}