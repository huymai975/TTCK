using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Models.ViewModels;

namespace WebAppBookingBoat.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- ĐĂNG KÝ ---

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.TenDangNhap,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.MatKhau!);

                if (result.Succeeded)
                {
                    // Đăng ký xong tự động đăng nhập luôn
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Nếu lỗi (ví dụ: mật khẩu quá ngắn, tên đã tồn tại...)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
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
                // false ở cuối là LockoutOnFailure: Không khóa tài khoản nếu nhập sai
                var result = await _signInManager.PasswordSignInAsync(model.TenDangNhap!, model.MatKhau!, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Chuyển hướng về trang trước đó nếu có, không thì về Home
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
                //if (result.IsLockedOut) return Content("Tài khoản bị khóa");
                //if (result.IsNotAllowed) return Content("Tài khoản không được phép (Có thể do chưa confirm email)");
                //if (result.RequiresTwoFactor) return Content("Cần xác thực 2 lớp");

                //ModelState.AddModelError("", "Sai tên hoặc mật khẩu. Kết quả: " + result.ToString());
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT ---

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}