using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            // 1. Lấy danh sách ID đánh giá mới nhất của mỗi khách hàng
            var uniqueDanhGiaIds = await _context.DanhGias
                .Where(d => d.TrangThai == "Đã hiển thị")
                .GroupBy(d => d.HoaDon!.MaKH)
                .Select(g => g.OrderByDescending(x => x.NgayDanhGia).Select(x => x.MaDanhGia).FirstOrDefault())
                .ToListAsync();

            // 2. Query chính - Dùng AsNoTracking để tăng tốc vì đây là trang hiển thị
            var query = _context.DanhGias
                .Include(d => d.HoaDon).ThenInclude(h => h!.KhachHang)
                .Include(d => d.HoaDon).ThenInclude(h => h!.Ves)
                    .ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.TuyenDuong)
                .Where(d => uniqueDanhGiaIds.Contains(d.MaDanhGia))
                .AsNoTracking() // Tối ưu performance
                .OrderByDescending(d => d.NgayDanhGia);

            int totalItems = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = pageNumber;

            var list = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DanhGiaViewModel
                {
                    MaDanhGia = d.MaDanhGia,
                    TenKhachHang = d.HoaDon!.KhachHang!.HoTen ?? "Khách hàng ẩn danh",
                    // Truy cập qua Ves (vì một hóa đơn vẫn có nhiều vé)
                    TenTuyenDuong = d.HoaDon.Ves.FirstOrDefault()!.LichTrinh!.TuyenDuong.TenTuyen,
                    SoSao = d.SoSao,
                    NoiDung = d.NoiDung,
                    HinhAnh = d.HinhAnh,
                    NgayDanhGia = d.NgayDanhGia,
                    PhanHoiAdmin = d.PhanHoiAdmin,
                    NgayPhanHoi = d.NgayPhanHoi
                })
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Phải Include đầy đủ như Index thì ViewModel mới có data TenTuyenDuong
            var d = await _context.DanhGias
                .Include(dg => dg.HoaDon).ThenInclude(h => h!.KhachHang)
                .Include(dg => dg.HoaDon).ThenInclude(h => h!.Ves)
                    .ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.TuyenDuong)
                .FirstOrDefaultAsync(dg => dg.MaDanhGia == id);

            if (d == null) return NotFound();

            var viewModel = new DanhGiaViewModel
            {
                MaDanhGia = d.MaDanhGia,
                MaHoaDon = d.MaHoaDon,
                TenKhachHang = d.HoaDon?.KhachHang?.HoTen ?? "Khách hàng ẩn danh",
                TenTuyenDuong = d.HoaDon?.Ves.FirstOrDefault()?.LichTrinh?.TuyenDuong.TenTuyen ?? "N/A",
                SoSao = d.SoSao,
                NoiDung = d.NoiDung,
                HinhAnh = d.HinhAnh,
                NgayDanhGia = d.NgayDanhGia,
                PhanHoiAdmin = d.PhanHoiAdmin,
                NgayPhanHoi = d.NgayPhanHoi
            };

            return View(viewModel);
        }
    }
}