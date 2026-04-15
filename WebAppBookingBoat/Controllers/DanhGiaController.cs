using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ đánh giá đã được duyệt hiển thị
            var list = await _context.DanhGias
                .Include(d => d.HoaDon)
                .Where(d => d.TrangThai == "Hiển thị")
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync();

            return View(list);
        }
    }
}
