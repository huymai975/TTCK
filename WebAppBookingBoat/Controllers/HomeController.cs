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

            // Lấy danh sách tuyến đường để hiển thị ảnh điểm đến
            model.TuyenDuongs = await _context.TuyenDuongs.Take(6).ToListAsync();

            // Lấy các lịch trình sắp tới (Sắp khởi hành)
            model.LichTrinhs = await _context.LichTrinhs
                .Include(l => l.TuyenDuong)
                .Include(l => l.Tau)
                .Where(l => l.NgayGioKhoiHanh >= DateTime.Now)
                .OrderBy(l => l.NgayGioKhoiHanh)
                .Take(6)
                .ToListAsync();

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
