//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using WebAppBookingBoat.Models;
//using WebAppBookingBoat.Repository;

//namespace WebAppBookingBoat.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class ErrorsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<AppUser> _userManager;

//        public ErrorsController(ApplicationDbContext context, UserManager<AppUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        [Route("Admin/Error/{statusCode}")]
//        public async Task<IActionResult> HttpStatusCodeHandler(int statusCode)
//        {
//            // Lấy URL mà người dùng đã cố gắng truy cập
//            var originalPath = HttpContext.Items["OriginalPath"]?.ToString() ?? Request.Path.ToString();
//            var userId = _userManager.GetUserId(User) ?? "Guest";

//            string errorDetail = $"Mã lỗi: {statusCode} tại URL: {originalPath}";
//            string logType = "Warning";

//            switch (statusCode)
//            {
//                case 404:
//                    ViewBag.ErrorMessage = "Xin lỗi, trang bạn tìm kiếm không tồn tại.";
//                    break;
//                case 403:
//                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập vào chức năng này.";
//                    logType = "Critical"; // Quyền truy cập bị từ chối là vấn đề bảo mật
//                    break;
//                case 500:
//                    ViewBag.ErrorMessage = "Hệ thống gặp sự cố bất ngờ. Vui lòng thử lại sau.";
//                    logType = "Error";
//                    break;
//                default:
//                    ViewBag.ErrorMessage = "Đã xảy ra lỗi không xác định.";
//                    break;
//            }

//            // Ghi log vào Database
//            var log = new Log
//            {
//                MaTK = userId,
//                HanhDong = "Lỗi hệ thống",
//                BangTacDong = "HTTP " + statusCode,
//                NoiDungChiTiet = errorDetail,
//                LoaiLog = logType,
//                ThoiGian = DateTime.Now,
//                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
//            };

//            _context.Logs.Add(log);
//            await _context.SaveChangesAsync();

//            return View("NotFound");
//        }
//    }
//}