using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Controllers // Không có .Areas.Admin
{
    public class ErrorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ErrorsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("Error/{statusCode}")]
        public async Task<IActionResult> HttpStatusCodeHandler(int statusCode)
        {
            // Lấy thông tin về yêu cầu gốc bị lỗi
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            string originalPath = "";
            string queryString = "";

            if (statusCodeReExecuteFeature != null)
            {
                // Đây chính là URL dẫn đến lỗi (Ví dụ: /Admin/Tau/Details/999)
                originalPath = statusCodeReExecuteFeature.OriginalPath;
                queryString = statusCodeReExecuteFeature.OriginalQueryString;
            }
            else
            {
                originalPath = HttpContext.Items["OriginalPath"]?.ToString() ?? Request.Path.ToString();
            }

            var userId = _userManager.GetUserId(User);

            // Ghi Log chi tiết hơn
            var log = new Log
            {
                MaTK = userId,
                HanhDong = "Lỗi HTTP " + statusCode,
                BangTacDong = "SystemError",
                // Ghi rõ Path và QueryString để bạn dễ debug
                NoiDungChiTiet = $"Lỗi tại: {originalPath}{queryString}. Trạng thái: {statusCode}",
                LoaiLog = statusCode >= 500 ? "Critical" : "Warning",
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            ViewBag.StatusCode = statusCode;
            ViewBag.OriginalPath = originalPath; // Truyền ra View để hiện cho user nếu cần

            // Logic chọn View và Layout của bạn giữ nguyên
            if (originalPath.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                return View("NotFound");
            }

            return View("GenericError");
        }
    }
}