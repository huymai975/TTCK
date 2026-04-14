using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // --- HÀM GHI LOG HỆ THỐNG (Đã sửa linh hoạt BangTacDong) ---
        private async Task GhiLogHeThong(string hanhDong, string bangTacDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = bangTacDong, // Linh hoạt theo tham số truyền vào
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        // --- ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByNameAsync(model.TenDangNhap!);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Tên tài khoản này đã tồn tại.");
                    return View(model);
                }

                var user = new AppUser { UserName = model.TenDangNhap, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.MatKhau!);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Khách hàng");

                    var khachHang = new KhachHang
                    {
                        MaTK = user.Id,
                        Email = user.Email!,
                        HoTen = model.HoTen!,
                        Sdt = model.SoDienThoai!
                    };

                    _context.KhachHangs.Add(khachHang);
                    await _context.SaveChangesAsync();

                    // Đăng nhập để ClaimsPrincipal (User) có giá trị trước khi ghi log
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    await GhiLogHeThong("Đăng ký", "KhachHangs", $"Khách hàng {model.TenDangNhap} đăng ký thành công.", "Info");

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.TenDangNhap!,
                    model.MatKhau!,
                    isPersistent: model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    await GhiLogHeThong("Đăng nhập", "AspNetUsers", $"Người dùng {model.TenDangNhap} đăng nhập.", "Info");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                await GhiLogHeThong("Đăng nhập thất bại", "AspNetUsers", $"Thử sai mật khẩu: {model.TenDangNhap}", "Warning");
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;

            // Ghi log trước khi hủy session
            await GhiLogHeThong("Đăng xuất", "AspNetUsers", $"Người dùng {userName} đăng xuất.", "Info");

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}