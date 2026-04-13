using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Hàm helper ghi log hệ thống
        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = "AspNetUsers",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        // 1. Danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // 2. Giao diện thêm mới
        public IActionResult Create() => View();

        // 3. Xử lý thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser model, string password, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError("", "Tên đăng nhập (Username) không được để trống.");
            }

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    TrangThai = model.TrangThai,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await GhiLogHeThong("Tạo người dùng", $"Tạo tài khoản mới: {username}");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // 4. Giao diện chỉnh sửa
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // 5. Xử lý cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUser model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.TrangThai = model.TrangThai;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await GhiLogHeThong("Cập nhật người dùng", $"Sửa tài khoản: {user.UserName}");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // 6. Đổi trạng thái nhanh (Sử dụng AJAX) - ĐÃ LỌC LỖI KHÓA CHÍNH MÌNH
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // KIỂM TRA BẢO MẬT:
            var currentUserId = _userManager.GetUserId(User);

            // 1. Không cho phép tự khóa chính mình
            if (user.Id == currentUserId)
                return Json(new { success = false, message = "Bạn không thể tự khóa tài khoản của chính mình!" });

            // 2. Không cho phép khóa Admin tổng
            if (user.UserName!.ToLower() == "admin")
                return Json(new { success = false, message = "Không thể thay đổi trạng thái của Admin hệ thống." });

            user.TrangThai = !user.TrangThai;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                string hanhDong = user.TrangThai ? "Kích hoạt" : "Vô hiệu hóa";
                await GhiLogHeThong(hanhDong, $"Thay đổi trạng thái tài khoản: {user.UserName}");
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái." });
        }

        // 7. Xóa mềm (Vô hiệu hóa) - ĐÃ LỌC LỖI KHÓA CHÍNH MÌNH
        [HttpPost]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // KIỂM TRA BẢO MẬT:
            var currentUserId = _userManager.GetUserId(User);

            // 1. Không cho phép tự vô hiệu hóa chính mình
            if (user.Id == currentUserId)
                return Json(new { success = false, message = "Bạn không thể vô hiệu hóa tài khoản đang sử dụng!" });

            // 2. Không cho phép vô hiệu hóa Admin hệ thống
            if (user.UserName!.ToLower() == "admin")
                return Json(new { success = false, message = "Không thể vô hiệu hóa Admin hệ thống." });

            user.TrangThai = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await GhiLogHeThong("Xóa mềm", $"Vô hiệu hóa tài khoản: {user.UserName}", "Warning");
                return Json(new { success = true, message = "Đã vô hiệu hóa tài khoản thành công." });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra khi thực hiện." });
        }
    }
}