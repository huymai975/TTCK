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
                    // Gán Role "Customer" (Tương ứng với ID 3: Khách hàng)
                    await _userManager.AddToRoleAsync(user, "Khách hàng");

                    // Tạo hồ sơ khách hàng tương ứng
                    var khachHang = new KhachHang { MaTK = user.Id, Email = user.Email! };
                    _context.KhachHangs.Add(khachHang);
                    await _context.SaveChangesAsync();


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
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie định danh của người dùng
            await _signInManager.SignOutAsync();

            // Điều hướng về trang chủ
            return RedirectToAction("Index", "Home", new {area = ""});
        }
    }
}