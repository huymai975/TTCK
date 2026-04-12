using System.ComponentModel.DataAnnotations;

namespace WebAppBookingBoat.ViewModels
{
    public class VeViewModel
    {
        public int MaVe { get; set; }

        [Display(Name = "Số ghế")]
        public string? TenGhe { get; set; }

        [Display(Name = "Hạng")]
        public string? LoaiGhe { get; set; }

        [Display(Name = "Tàu")]
        public string? TenTau { get; set; }

        [Display(Name = "Chuyến đi")]
        public string? ThongTinChuyen { get; set; }

        [Display(Name = "Khách hàng")]
        public string? TenKhachHang { get; set; }

        [Display(Name = "Mã HĐ")]
        public int MaHoaDon { get; set; }

        [Display(Name = "Giá vé")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal GiaVe { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }


        public string? TrangThaiHoaDon { get; set; } // Thêm dòng này


        // Hỗ trợ hiển thị màu sắc Badge trên View
        public string ClassTrangThai => TrangThai switch
        {
            "Hợp lệ" => "success",
            "Đã sử dụng" => "info",
            "Đã hủy" => "danger",
            _ => "secondary"
        };
    }
}