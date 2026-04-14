using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // QUAN TRỌNG: Thêm dòng này để sửa lỗi FirstOrDefaultAsync
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Tìm trong bảng KhachHang xem có bản ghi ứng với tài khoản này không
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
            var user = await _userManager.FindByIdAsync(userId!);

            // LOGIC KIỂM TRA:
            // Nếu là Admin/Nhân viên (không có trong bảng KhachHang)
            if (khachHang == null)
            {
                // Bạn có thể redirect về trang Profile dành cho nội bộ hoặc thông báo
                // Ở đây tôi tạm thời khởi tạo model rỗng từ Identity để tránh crash
                var adminModel = new ProfileVM
                {
                    TenDangNhap = user?.UserName,
                    Email = user?.Email ?? "",
                    HoTen = "Nhân viên hệ thống",
                    Sdt = user?.PhoneNumber ?? ""
                };
                return View(adminModel);
            }

            // Nếu là Khách hàng
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
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            // Tìm thông tin khách hàng
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);

            // 1. CẬP NHẬT IDENTITY (AppUser)
            // Luôn cập nhật Email và Sdt ở Identity để đồng bộ hệ thống
            user.Email = model.Email;
            user.UserName = user.UserName; // Giữ nguyên UserName hoặc cho phép đổi tùy bạn
            user.PhoneNumber = model.Sdt;

            var resultIdentity = await _userManager.UpdateAsync(user);

            if (resultIdentity.Succeeded)
            {
                // 2. CẬP NHẬT BẢNG KHACHHANG (Nếu là khách hàng)
                if (khachHang != null)
                {
                    khachHang.HoTen = model.HoTen;
                    khachHang.Sdt = model.Sdt;
                    khachHang.Email = model.Email;
                    khachHang.NgaySinh = model.NgaySinh;

                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();
                }

                // 3. GHI LOG (Sử dụng hàm GhiLog bạn đã tạo ở AccountController hoặc đưa vào BaseController)
                // await GhiLog(userId, "Cập nhật hồ sơ", "Người dùng đã thay đổi thông tin cá nhân thành công");

                TempData["Success"] = "Thông tin của bạn đã được cập nhật thành công!";
            }
            else
            {
                // Trả về lỗi nếu Identity không cho cập nhật (ví dụ Email đã tồn tại)
                TempData["Error"] = resultIdentity.Errors.FirstOrDefault()?.Description ?? "Có lỗi xảy ra khi cập nhật.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileVM model)
        {
            if (string.IsNullOrEmpty(model.OldPassword) || string.IsNullOrEmpty(model.NewPassword))
            {
                TempData["ErrorPass"] = "Vui lòng nhập đầy đủ thông tin mật khẩu.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessPass"] = "Đổi mật khẩu thành công!";
            }
            else
            {
                // Lấy lỗi từ Identity trả về (ví dụ: mật khẩu cũ sai)
                TempData["ErrorPass"] = result.Errors.FirstOrDefault()?.Description ?? "Đổi mật khẩu thất bại.";
            }

            return RedirectToAction("Index");
        }
    }
}