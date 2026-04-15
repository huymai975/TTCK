using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhuyenMaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            // Bước 1: Lấy dữ liệu về RAM (Fix điều kiện TrangThai khớp với DB)
            var dataFromDb = await _context.KhuyenMais
                .Include(k => k.HoaDons)
                // Sửa ở đây: Lấy cả "Đang diễn ra" và "Sắp diễn ra"
                .Where(k => (k.TrangThai == "Đang diễn ra" || k.TrangThai == "Sắp diễn ra")
                             && k.NgayKetThuc >= now)
                .ToListAsync();

            // Bước 2: Map sang ViewModel
            var result = dataFromDb.Select(k => new KhuyenMaiViewModel
            {
                MaKM = k.MaKM,
                TenChuongTrinh = k.TenChuongTrinh,
                HinhAnh = k.HinhAnh,
                MoTa = k.MoTa,
                PhanTramGiam = k.PhanTramGiam,
                SoTienToiDaGiam = k.SoTienToiDaGiam,
                NgayKetThuc = k.NgayKetThuc,
                SoLuotDaDung = k.HoaDons?.Count ?? 0
            })
            .OrderByDescending(k => k.NgayKetThuc)
            .ToList();

            return View(result);
        }

        // Action xem chi tiết 1 mã khuyến mãi
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var km = await _context.KhuyenMais
                .Include(k => k.HoaDons)
                .FirstOrDefaultAsync(m => m.MaKM == id);

            if (km == null) return NotFound();

            var viewModel = new KhuyenMaiViewModel
            {
                MaKM = km.MaKM,
                TenChuongTrinh = km.TenChuongTrinh,
                HinhAnh = km.HinhAnh,
                MoTa = km.MoTa,
                PhanTramGiam = km.PhanTramGiam,
                SoTienToiDaGiam = km.SoTienToiDaGiam,
                NgayKetThuc = km.NgayKetThuc,
                SoLuotDaDung = km.HoaDons.Count
            };

            return View(viewModel);
        }
    }
}