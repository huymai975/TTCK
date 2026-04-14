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
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        // Hàm ghi log hệ thống
        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = "Roles", // Ghi rõ bảng tác động là Roles
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                roleName = roleName.Trim();
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        await GhiLogHeThong("Tạo quyền", $"Đã tạo quyền mới: {roleName}", "Success");
                        TempData["Success"] = "Tạo quyền thành công!";
                    }
                    else
                    {
                        await GhiLogHeThong("Tạo quyền", $"Lỗi khi tạo quyền {roleName}", "Error");
                        TempData["Error"] = "Có lỗi xảy ra khi tạo quyền.";
                    }
                }
                else
                {
                    TempData["Error"] = "Quyền này đã tồn tại!";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Không tìm thấy quyền này." });
            }

            if (role.Name == "Admin")
            {
                return Json(new { success = false, message = "Không thể xóa quyền Admin tối cao!" });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                await GhiLogHeThong("Xóa quyền", $"Đã xóa quyền: {role.Name} (ID: {id})", "Warning");
                return Json(new { success = true, message = "Xóa quyền thành công!" });
            }
            else
            {
                await GhiLogHeThong("Xóa quyền", $"Thất bại khi xóa quyền: {role.Name}", "Error");
                return Json(new { success = false, message = "Lỗi: Không thể xóa quyền đang có người sử dụng." });
            }
        }
    }
}