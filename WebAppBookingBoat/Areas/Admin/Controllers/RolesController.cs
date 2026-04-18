using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebAppBookingBoat.Areas.Admin.ViewModels;
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


        // 1. Hiển thị danh sách người dùng và quyền hiện tại của họ
        public async Task<IActionResult> UserRoles()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                // Giả sử AppUser của bạn có property FullName, 
                // hoặc bạn cần Join với bảng KhachHang/NhanVien để lấy tên.
                // Ở đây tôi lấy trực tiếp từ user (nếu AppUser có FullName)
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email!,
                    Roles = await _userManager.GetRolesAsync(user)
                };
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }

        // 2. Thay đổi quyền của người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeToStaff(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản!";
                return RedirectToAction(nameof(UserRoles));
            }

            // Khởi tạo transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật Role sang Staff (Xóa các role cũ để tránh xung đột)
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, "Staff");

                // 2. Xử lý hồ sơ Nhân viên & Khách hàng
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
                var nhanVienTonTai = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaTK == userId);

                if (nhanVienTonTai == null)
                {
                    var nhanVienMoi = new WebAppBookingBoat.Models.NhanVien
                    {
                        MaTK = userId,
                        // Lấy thông tin từ hồ sơ KH hoặc từ Identity User
                        HoTen = (khachHang?.HoTen ?? user.UserName ?? "Nhân viên mới").Trim(),
                        Sdt = khachHang?.Sdt ?? user.PhoneNumber ?? "0123456789",
                        Email = user.Email ?? "default@booking.com",
                        ChucVu = "Nhân viên",
                        Luong = 0,
                        TrangThai = true
                    };
                    _context.NhanViens.Add(nhanVienMoi);
                }

                // 3. Xóa hồ sơ khách hàng cũ nếu có
                if (khachHang != null)
                {
                    _context.KhachHangs.Remove(khachHang);
                }

                // 4. Lưu tất cả thay đổi vào Database
                await _context.SaveChangesAsync();

                // 5. Xác nhận hoàn tất giao dịch
                await transaction.CommitAsync();

                TempData["Success"] = $"Đã nâng cấp {user.UserName} thành Nhân viên thành công!";
            }
            catch (Exception ex)
            {
                // GIẢI PHÁP SỬA LỖI ZOMBIE: 
                // Kiểm tra xem Connection có còn mở không trước khi gọi Rollback
                // Nếu SQL Server đã tự Abort transaction do lỗi ràng buộc, ta không cần Rollback nữa
                if (transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }

                // Log lỗi chi tiết ra console để bạn debug
                Console.WriteLine("Lỗi chi tiết: " + ex.ToString());

                // Trả lỗi về giao diện
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi hệ thống: " + errorMessage;
            }

            return RedirectToAction(nameof(UserRoles));
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            ViewBag.UserName = user.UserName;
            ViewBag.CurrentUserId = userId;

            var roles = await _roleManager.Roles.ToListAsync();
            var model = new List<ManageUserRolesViewModel>();

            foreach (var role in roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    Selected = await _userManager.IsInRoleAsync(user, role.Name!)
                };
                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Không thể xóa quyền cũ.";
                return View(model);
            }

            var selectedRoles = model.Where(x => x.Selected).Select(y => y.RoleName).ToList();
            if (selectedRoles.Any())
            {
                result = await _userManager.AddToRolesAsync(user, selectedRoles);
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Lỗi khi thêm quyền mới.";
                    return View(model);
                }
            }

            await GhiLogHeThong("Cập nhật quyền", $"Admin thay đổi quyền cho: {user.UserName}", "Warning");
            TempData["Success"] = "Cập nhật quyền thành công!";

            return RedirectToAction(nameof(UserRoles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileType(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // 1. Cập nhật Role trong Identity
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            // 2. Cập nhật hồ sơ chi tiết (Profile)
            if (newRole == "Staff" || newRole.Equals("Nhân viên")) // Nếu chuyển sang Nhân viên
            {
                // Kiểm tra xem đã có hồ sơ Khách hàng chưa
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == userId);
                if (khachHang != null)
                {
                    // Tạo mới hồ sơ Nhân viên (lấy dữ liệu từ Khách hàng sang nếu cần)
                    var nhanVien = new WebAppBookingBoat.Models.NhanVien
                    {
                        MaTK = userId,
                        HoTen = khachHang.HoTen, // Copy tên sang
                        Sdt = khachHang.Sdt,
                        TrangThai = true
                    };

                    _context.NhanViens.Add(nhanVien);
                    _context.KhachHangs.Remove(khachHang); // Xóa hồ sơ khách hàng cũ
                }
            }
            else if (newRole == "Customer" || newRole.Equals("Khách hàng")) // Nếu chuyển ngược lại sang Khách hàng
            {
                var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaTK == userId);
                if (nhanVien != null)
                {
                    var khachHang = new KhachHang
                    {
                        MaTK = userId,
                        HoTen = nhanVien.HoTen,
                        Sdt = nhanVien.Sdt,
                    };

                    _context.KhachHangs.Add(khachHang);
                    _context.NhanViens.Remove(nhanVien);
                }
            }

            await _context.SaveChangesAsync();
            await GhiLogHeThong("Chuyển loại hồ sơ", $"Chuyển {user.UserName} sang {newRole}", "Warning");

            TempData["Success"] = "Đã cập nhật Role và chuyển đổi hồ sơ thành công!";
            return RedirectToAction(nameof(UserRoles));
        }
    }
}