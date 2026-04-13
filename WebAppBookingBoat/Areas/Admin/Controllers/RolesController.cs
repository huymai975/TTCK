using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bảo vệ controller này, chỉ Admin mới được quản lý Role
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        #region Helpers
        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = "AspNetRoles",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
        #endregion

        // 1. Danh sách các quyền
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // 2. Tạo mới quyền
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Tên quyền không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            var roleExist = await _roleManager.RoleExistsAsync(roleName.Trim());
            if (!roleExist)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
                if (result.Succeeded)
                {
                    await GhiLogHeThong("Tạo quyền mới", $"Đã tạo quyền: {roleName}");
                    TempData["Success"] = "Thêm quyền thành công!";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo quyền.";
                }
            }
            else
            {
                TempData["Error"] = "Tên quyền này đã tồn tại!";
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Xóa quyền
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Kiểm tra xem có User nào đang giữ quyền này không trước khi xóa (Tùy chọn)
            // Nếu muốn chặt chẽ, bạn nên ngăn xóa nếu Role đang có người dùng.

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                await GhiLogHeThong("Xóa quyền", $"Đã xóa quyền: {role.Name}", "Warning");
                TempData["Success"] = "Đã xóa quyền thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể xóa quyền này.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 4. Cập nhật tên quyền (Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string newRoleName)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(newRoleName))
            {
                string oldName = role.Name!;
                role.Name = newRoleName.Trim();
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    await GhiLogHeThong("Cập nhật quyền", $"Đổi tên quyền từ {oldName} sang {newRoleName}");
                    TempData["Success"] = "Cập nhật tên quyền thành công.";
                }
                else
                {
                    TempData["Error"] = "Cập nhật thất bại.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}