using Microsoft.AspNetCore.Mvc;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ErrorsController : Controller
    {

        [Route("Admin/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Xin lỗi, trang bạn tìm kiếm không tồn tại.";
                    break;
                    // Bạn có thể thêm các mã lỗi khác như 500, 403 tại đây
            }
            return View("NotFound");
        }
    }
}
