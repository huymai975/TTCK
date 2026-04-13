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
            var originalPath = HttpContext.Items["OriginalPath"]?.ToString() ?? Request.Path.ToString();
            var userId = _userManager.GetUserId(User);

            // --- LOGIC GHI LOG VẪN GIỮ NGUYÊN ---
            var log = new Log
            {
                MaTK = userId,
                HanhDong = "Lỗi hệ thống",
                BangTacDong = "HTTP " + statusCode,
                NoiDungChiTiet = $"Lỗi {statusCode} tại {originalPath}",
                LoaiLog = statusCode == 403 ? "Critical" : "Warning",
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
            // ------------------------------------

            ViewBag.StatusCode = statusCode;

            ViewBag.ErrorMessage = statusCode switch
            {
                404 => "Trang bạn tìm kiếm không tồn tại.",
                403 => "Bạn không có quyền truy cập chức năng này.",
                500 => "Hệ thống đang bảo trì, vui lòng thử lại sau.",
                _ => "Đã xảy ra lỗi không xác định."
            };

            // TỰ ĐỘNG CHỌN LAYOUT
            // Nếu URL chứa chữ "/Admin", nó sẽ tìm View kèm Layout Admin (nếu bạn cấu hình View chuẩn)
            if (originalPath.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                return View("NotFound"); // Tạo file AdminError.cshtml trong Views/Errors
            }

            return View("GenericError"); // Tạo file GenericError.cshtml trong Views/Errors
        }
    }
}