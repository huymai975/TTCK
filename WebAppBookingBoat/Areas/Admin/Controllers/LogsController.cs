using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LogsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Logs
        public async Task<IActionResult> Index(string searchString, string loaiLog)
        {
            // Lấy danh sách log và nạp thông tin User liên quan
            var logsQuery = _context.Logs
                .Include(l => l.AppUser)
                .OrderByDescending(l => l.ThoiGian) // Log mới nhất hiện lên đầu
                .AsQueryable();

            // Bộ lọc theo loại (Info/Warning/Error)
            if (!string.IsNullOrEmpty(loaiLog))
            {
                logsQuery = logsQuery.Where(l => l.LoaiLog == loaiLog);
            }

            // Bộ lọc tìm kiếm theo hành động hoặc nội dung
            if (!string.IsNullOrEmpty(searchString))
            {
                // Sử dụng toán tử ?. hoặc kiểm tra null như trên
                logsQuery = logsQuery.Where(l => (l.HanhDong != null && l.HanhDong.Contains(searchString))
                                              || (l.NoiDungChiTiet != null && l.NoiDungChiTiet.Contains(searchString)));
            }

            return View(await logsQuery.ToListAsync());
        }

        // GET: Admin/Logs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var log = await _context.Logs
                .Include(l => l.AppUser)
                .FirstOrDefaultAsync(m => m.MaLog == id);

            if (log == null) return NotFound();

            return View(log);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var log = await _context.Logs.FindAsync(id);
                if (log == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi nhật ký này." });
                }

                _context.Logs.Remove(log);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa bản ghi nhật ký thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [NonAction]
        private async Task GhiLogHeThong(string hanhDong, string bang, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User), // Lấy ID admin đang đăng nhập
                HanhDong = hanhDong,
                BangTacDong = bang,
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}