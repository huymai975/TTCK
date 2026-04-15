using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel();

            model.TuyenDuongs = await _context.TuyenDuongs.Take(6).ToListAsync();

            model.LichTrinhs = await _context.LichTrinhs
                .Include(l => l.TuyenDuong).Include(l => l.Tau)
                .Where(l => l.NgayGioKhoiHanh >= DateTime.Now)
                .Take(6).ToListAsync();

            // Logic: Lọc mỗi khách hàng chỉ lấy 1 đánh giá mới nhất
            model.DanhGias = await _context.DanhGias
                .Include(d => d.HoaDon).ThenInclude(h => h!.KhachHang)
                .Include(d => d.HoaDon).ThenInclude(h => h!.Ves)
                    .ThenInclude(v => v.LichTrinh).ThenInclude(l => l!.TuyenDuong)
                .Where(d => d.TrangThai == "Đã hiển thị")
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync(); // Lấy hết về bộ nhớ để lọc GroupBy dễ hơn

            model.DanhGias = model.DanhGias
                .GroupBy(d => d.HoaDon?.MaKH) // Nhóm theo mã khách hàng
                .Select(g => g.First()) // Lấy đánh giá đầu tiên (mới nhất) của mỗi nhóm
                .Take(3) // Chỉ lấy 3 người khác nhau
                .ToList();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
