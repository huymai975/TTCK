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
            // Sử dụng AsNoTracking để tăng tốc truy vấn đọc
            var logsQuery = _context.Logs
                .AsNoTracking()
                .Include(l => l.AppUser)
                .OrderByDescending(l => l.ThoiGian)
                .AsQueryable();

            // Bộ lọc theo loại
            if (!string.IsNullOrEmpty(loaiLog))
            {
                logsQuery = logsQuery.Where(l => l.LoaiLog == loaiLog);
            }

            // Bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                logsQuery = logsQuery.Where(l => (l.HanhDong != null && l.HanhDong.Contains(searchString))
                                              || (l.NoiDungChiTiet != null && l.NoiDungChiTiet.Contains(searchString))
                                              || (l.AppUser!.UserName != null && l.AppUser.UserName.Contains(searchString)));
            }

            // QUAN TRỌNG: Chỉ lấy 300 bản ghi mới nhất để Index luôn mượt
            var result = await logsQuery.Take(300).ToListAsync();

            return View(result);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteByRange(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Đảm bảo lấy hết dữ liệu trong ngày toDate bằng cách so sánh đến 23:59:59
                var logsToDelete = _context.Logs
                    .Where(l => l.ThoiGian.Date >= fromDate.Date && l.ThoiGian.Date <= toDate.Date);

                int count = await logsToDelete.CountAsync(); // Lấy số lượng trước khi xóa

                if (count > 0)
                {
                    _context.Logs.RemoveRange(logsToDelete);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = $"Đã dọn dẹp thành công {count} bản ghi nhật ký." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
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