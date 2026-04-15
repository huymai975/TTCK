using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Models.ViewModels;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task GhiLogHeThong(string hanhDong, string bangTacDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = bangTacDong,
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);

            // Logic cho Admin/Nhân viên không có trong bảng KhachHang
            if (khachHang == null)
            {
                var adminModel = new ProfileVM
                {
                    TenDangNhap = user?.UserName,
                    Email = user?.Email ?? "",
                    HoTen = "Nhân viên hệ thống",
                    Sdt = user?.PhoneNumber ?? ""
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
            if (!ModelState.IsValid)
            {
                // Xóa các lỗi validation liên quan đến mật khẩu vì mình đang ở hàm UpdateInfo
                // Việc này giúp tránh việc báo lỗi "Mật khẩu không được để trống" khi đang sửa tên
                ModelState.Remove("OldPassword");
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmPassword");

                if (!ModelState.IsValid) return View("Index", model);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId!);
                if (user == null) return NotFound();

                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);

                // 1. Cập nhật bảng Identity (AspNetUsers)
                user.Email = model.Email;
                user.PhoneNumber = model.Sdt;
                var resultIdentity = await _userManager.UpdateAsync(user);

                if (resultIdentity.Succeeded)
                {
                    // 2. Cập nhật bảng KhachHang (nếu có)
                    if (khachHang != null)
                    {
                        khachHang.HoTen = model.HoTen;
                        khachHang.Sdt = model.Sdt;
                        khachHang.Email = model.Email;
                        khachHang.NgaySinh = model.NgaySinh;

                        _context.Update(khachHang);
                        await _context.SaveChangesAsync();
                    }

                    await GhiLogHeThong("Cập nhật hồ sơ", "KhachHangs/AspNetUsers", $"Người dùng {user.UserName} cập nhật thông tin thành công", "Info");
                    TempData["SuccessMessage"] = "Thông tin cá nhân của bạn đã được cập nhật!";
                }
                else
                {
                    TempData["ErrorMessage"] = resultIdentity.Errors.FirstOrDefault()?.Description ?? "Lỗi cập nhật tài khoản.";
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