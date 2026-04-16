using Microsoft.AspNetCore.Mvc;

namespace WebAppBookingBoat.Areas.NhanVien.Controllers
{
    public class HomeController : Controller
    {
        [Area("Nhân viên")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
