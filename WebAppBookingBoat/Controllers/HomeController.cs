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

            // 1. Lấy danh sách tuyến đường
            model.TuyenDuongs = await _context.TuyenDuongs.Take(6).ToListAsync();

            // 2. Lấy lịch trình và tính toán thông tin liên quan (Tàu, Tuyến, Vé đã đặt)
            // Cần Include "Tau" để biết tổng số ghế và "Ves" để đếm số lượng đã bán
            model.LichTrinhs = await _context.LichTrinhs
                .Include(l => l.TuyenDuong)
                .Include(l => l.Tau)
                .Include(l => l.Ves) // Cần thiết để tính Số Ghế Trống
                .Where(l => l.NgayGioKhoiHanh >= DateTime.Now && l.TrangThai == "Sắp khởi hành")
                .OrderBy(l => l.NgayGioKhoiHanh)
                .Take(6)
                .ToListAsync();

            // 3. Logic lấy đánh giá (Giữ nguyên logic GroupBy của bạn nhưng tối ưu Include)
            var allDanhGias = await _context.DanhGias
                .Include(d => d.HoaDon)
                    .ThenInclude(h => h!.KhachHang)
                .Include(d => d.HoaDon)
                    .ThenInclude(h => h!.Ves)
                        .ThenInclude(v => v.LichTrinh)
                            .ThenInclude(l => l!.TuyenDuong)
                .Where(d => d.TrangThai == "Đã hiển thị")
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync();

            model.DanhGias = allDanhGias
                .GroupBy(d => d.HoaDon?.MaKH)
                .Select(g => g.First())
                .Take(3)
                .ToList();

            return View(model);
        }

        public IActionResult About()
        {
            ViewBag.Title = "Về chúng tôi - Water Boat Booking";
            return View();
        }
        public IActionResult Service()
        {
            ViewBag.Title = "Dịch vụ của chúng tôi - Water Boat Booking";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Title = "Liên hệ với chúng tôi - Water Boat Booking";
            return View();
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
