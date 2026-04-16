using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            if (!ModelState.IsValid) return View(model);

            try
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

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await GhiLogHeThong("Đăng ký", "KhachHangs", $"Khách hàng {model.TenDangNhap} đăng ký thành công.", "Info");

                    // --- THÊM THÔNG BÁO ---
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Chào mừng bạn đến với TTCK.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.";
                await GhiLogHeThong("Lỗi đăng ký", "System", ex.Message, "Error");
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
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.TenDangNhap!,
                model.MatKhau!,
                isPersistent: model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                await GhiLogHeThong("Đăng nhập", "AspNetUsers", $"Người dùng {model.TenDangNhap} đăng nhập.", "Info");

                TempData["SuccessMessage"] = "Chào mừng bạn quay trở lại!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Xử lý khi đăng nhập thất bại
            await GhiLogHeThong("Đăng nhập thất bại", "AspNetUsers", $"Thử sai mật khẩu: {model.TenDangNhap}", "Warning");
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            TempData["ErrorMessage"] = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";

            return View(model);
        }

        // --- ĐĂNG XUẤT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
            var nhanvien = await _context.NhanViens.FirstOrDefaultAsync(k => k.MaTK == userId);

            // Logic cho Admin/Nhân viên không có trong bảng KhachHang
            if (khachHang == null)
            {
                var adminModel = new ProfileVM
                {
                    TenDangNhap = user?.UserName,
                    Email = nhanvien?.Email ?? "",
                    HoTen = nhanvien?.HoTen ?? "",
                    Sdt = nhanvien?.Sdt ?? ""
                };
                return View(adminModel);
            }

            var model = new ProfileVM
            {
                MaKH = khachHang.MaKH,
                HoTen = khachHang.HoTen,
                Sdt = khachHang.Sdt,
                Email = khachHang.Email,
                NgaySinh = khachHang.NgaySinh,
                TenDangNhap = user?.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(ProfileVM model)
        {
            // Loại bỏ kiểm tra mật khẩu vì đây là hàm cập nhật thông tin cơ bản
            ModelState.Remove("OldPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid) return View("Index", model);

            try
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId!);
                if (user == null) return NotFound();

                // 1. Cập nhật bảng Identity (AspNetUsers)
                user.Email = model.Email;
                user.PhoneNumber = model.Sdt;
                var resultIdentity = await _userManager.UpdateAsync(user);

                if (!resultIdentity.Succeeded)
                {
                    TempData["ErrorMessage"] = resultIdentity.Errors.FirstOrDefault()?.Description ?? "Lỗi cập nhật tài khoản.";
                    return RedirectToAction("Index");
                }

                bool isUpdated = false;

                // 2. KIỂM TRA VÀ CẬP NHẬT THEO ROLE / BẢNG TƯƠNG ỨNG

                // Kiểm tra xem có phải Khách hàng không
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
                if (khachHang != null)
                {
                    khachHang.HoTen = model.HoTen;
                    khachHang.Sdt = model.Sdt;
                    khachHang.Email = model.Email;
                    khachHang.NgaySinh = model.NgaySinh;
                    _context.Update(khachHang);
                    isUpdated = true;
                }
                else
                {
                    // Nếu không phải khách hàng, kiểm tra xem có phải Nhân viên không
                    var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.MaTK == userId);
                    if (nhanVien != null)
                    {
                        nhanVien.HoTen = model.HoTen;
                        nhanVien.Sdt = model.Sdt;
                        nhanVien.Email = model.Email;
                        // Nếu bảng nhân viên có ngày sinh thì cập nhật, không thì bỏ qua
                        // nhanVien.NgaySinh = model.NgaySinh; 

                        _context.Update(nhanVien);
                        isUpdated = true;
                    }
                }

                if (isUpdated)
                {
                    await _context.SaveChangesAsync();
                    await GhiLogHeThong("Cập nhật hồ sơ", "ProfileUpdate", $"Người dùng {user.UserName} cập nhật thành công", "Info");
                    TempData["SuccessMessage"] = "Thông tin của bạn đã được cập nhật thành công!";
                }
                else
                {
                    // Trường hợp tài khoản Admin gốc (không thuộc bảng KH cũng không thuộc NV)
                    TempData["SuccessMessage"] = "Thông tin tài khoản Identity đã được cập nhật.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình lưu dữ liệu.";
                await GhiLogHeThong("Lỗi UpdateInfo", "System", ex.Message, "Error");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // 1. Loại bỏ các trường không liên quan để tránh báo lỗi oan bên form Hồ sơ
            ModelState.Remove("HoTen");
            ModelState.Remove("Sdt");
            ModelState.Remove("Email");
            ModelState.Remove("NgaySinh");

            // 2. Kiểm tra Validation (Lỗi thẻ span hiện ở đây)
            if (!ModelState.IsValid)
            {
                await PopulateUserInfo(model); // Hàm nạp lại dữ liệu hồ sơ bạn đã viết
                return View("Index", model); // Trả về View để hiện lỗi đỏ dưới thẻ span
            }

            try
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!);

                if (result.Succeeded)
                {
                    // THÀNH CÔNG: Thông báo qua TempData (Modal/Toast)
                    await GhiLogHeThong("Đổi mật khẩu", "AspNetUsers", $"Người dùng {user.UserName} đổi mật khẩu thành công", "Info");
                    TempData["SuccessMessage"] = "Mật khẩu của bạn đã được thay đổi thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    // THẤT BẠI: Nếu là lỗi nghiệp vụ (như sai mật khẩu cũ), đưa nó vào ModelState 
                    // để nó hiện thành chữ đỏ dưới thẻ span luôn cho đồng bộ, không dùng TempData ở đây.
                    foreach (var error in result.Errors)
                    {
                        if (error.Code == "PasswordMismatch")
                        {
                            ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                        }
                        else
                        {
                            ModelState.AddModelError("NewPassword", error.Description);
                        }
                    }

                    await GhiLogHeThong("Đổi mật khẩu thất bại", "AspNetUsers", $"User {user.UserName} nhập sai mật khẩu cũ", "Warning");

                    // Trả về View để hiện lỗi đỏ dưới ô input (không redirect)
                    await PopulateUserInfo(model);
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống khi đổi mật khẩu.";
                await GhiLogHeThong("Lỗi ChangePassword", "System", ex.Message, "Error");
                return RedirectToAction("Index");
            }
        }

        private async Task PopulateUserInfo(ProfileVM model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return;

            // Lấy thông tin từ AspNetUsers (Identity)
            var user = await _userManager.FindByIdAsync(userId);

            // Lấy thông tin từ bảng KhachHang
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);

            if (user != null)
            {
                model.TenDangNhap = user.UserName;
                // Nếu model đang trống (do lỗi validation), nạp lại từ DB
                if (string.IsNullOrEmpty(model.Email)) model.Email = user.Email ?? "";
                if (string.IsNullOrEmpty(model.Sdt)) model.Sdt = user.PhoneNumber ?? "";
            }

            if (khachHang != null)
            {
                if (string.IsNullOrEmpty(model.HoTen)) model.HoTen = khachHang.HoTen;
                if (model.NgaySinh == null) model.NgaySinh = khachHang.NgaySinh;
                model.MaKH = khachHang.MaKH;
            }
        }
    }
}