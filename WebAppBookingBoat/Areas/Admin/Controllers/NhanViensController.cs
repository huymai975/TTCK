using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NhanViensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NhanViensController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // HÀM LOGIC HỖ TRỢ (PRIVATE)
        // ==========================================

        private async Task<string?> CheckNhanVienLogic(WebAppBookingBoat.Models.NhanVien nv, int? id = null)
        {
            if (nv.Luong < 0) return "Lương không được phép là số âm.";

            // 1. Kiểm tra Email trùng trong bảng NhanViens
            if (await _context.NhanViens.AnyAsync(n => n.Email == nv.Email && n.MaNV != id))
                return "Email này đã tồn tại trong danh sách nhân viên!";

            // 2. Kiểm tra Email trùng trong bảng Users hệ thống (Identity)
            // Nếu nhân viên không liên kết tài khoản (MaTK null), vẫn nên chặn nếu Email đó đã có người dùng khác đăng ký
            var userWithEmail = await _userManager.FindByEmailAsync(nv.Email);
            if (userWithEmail != null && userWithEmail.Id != nv.MaTK)
                return "Email này đã được sử dụng bởi một tài khoản khác trong hệ thống!";

            // 3. Kiểm tra Số điện thoại trùng trong bảng NhanViens
            if (await _context.NhanViens.AnyAsync(n => n.Sdt == nv.Sdt && n.MaNV != id))
                return "Số điện thoại này đã tồn tại cho một nhân viên khác!";

            // 4. KIỂM TRA CHÉO MaTK (Tài khoản liên kết)
            if (!string.IsNullOrEmpty(nv.MaTK))
            {
                // Kiểm tra xem MaTK có đang được dùng bởi nhân viên khác không
                if (await _context.NhanViens.AnyAsync(n => n.MaTK == nv.MaTK && n.MaNV != id))
                    return "Tài khoản hệ thống này đã được gán cho nhân viên khác!";

                // Kiểm tra xem MaTK có phải là khách hàng không
                if (await _context.KhachHangs.AnyAsync(kh => kh.MaTK == nv.MaTK))
                    return "Tài khoản này thuộc về một Khách hàng, không thể gán làm Nhân viên!";
            }

            return null;
        }

        private void LoadUserData(int? currentMaNV = null, string? selectedMaTK = null)
        {
            // 1. Lấy danh sách ID từ bảng Nhân viên (Tải về List<string>)
            var assignedInStaff = _context.NhanViens
                .Where(n => n.MaTK != null && n.MaNV != currentMaNV)
                .Select(n => n.MaTK)
                .ToList();

            // 2. Lấy danh sách ID từ bảng Khách hàng (Tải về List<string>)
            var assignedInCustomer = _context.KhachHangs
                .Where(k => k.MaTK != null)
                .Select(k => k.MaTK)
                .ToList();

            // 3. Hợp nhất và loại bỏ trùng lặp trong bộ nhớ
            var allAssignedIds = assignedInStaff
                .Union(assignedInCustomer)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            // 4. Lọc Users dựa trên danh sách ID đã thu thập
            var availableUsers = _context.Users
                .Where(u => !allAssignedIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    Display = u.UserName + " (" + u.Email + ")"
                })
                .ToList();

            // 5. Nếu đang ở chế độ EDIT, phải đảm bảo tài khoản hiện tại được hiển thị lại
            if (!string.IsNullOrEmpty(selectedMaTK))
            {
                var currentUser = _context.Users
                    .Where(u => u.Id == selectedMaTK)
                    .Select(u => new { u.Id, Display = u.UserName + " (" + u.Email + ")" })
                    .FirstOrDefault();

                if (currentUser != null && !availableUsers.Any(x => x.Id == selectedMaTK))
                {
                    availableUsers.Add(currentUser);
                }
            }

            ViewData["MaTK"] = new SelectList(availableUsers, "Id", "Display", selectedMaTK);
        }

        // ==========================================
        // CÁC ACTIONS (PUBLIC)
        // ==========================================

        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.NhanViens.Include(n => n.AppUser).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.HoTen.Contains(searchString) || n.Email.Contains(searchString));
                ViewBag.Search = searchString;
            }
            return View(await query.OrderByDescending(n => n.MaNV).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var nhanVien = await _context.NhanViens.Include(n => n.AppUser).FirstOrDefaultAsync(m => m.MaNV == id);
            if (nhanVien == null) return NotFound();
            return View(nhanVien);
        }

        public IActionResult Create()
        {
            LoadUserData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebAppBookingBoat.Models.NhanVien nhanVien)
        {
            ModelState.Remove("AppUser");
            ModelState.Remove("HoaDons");

            if (string.IsNullOrWhiteSpace(nhanVien.MaTK)) nhanVien.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckNhanVienLogic(nhanVien);
                if (error == null)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Add(nhanVien);
                        await _context.SaveChangesAsync();

                        await GhiLogHeThong("Thêm nhân viên", "NhanViens",
                            $"Admin đã tạo hồ sơ nhân viên cho: {nhanVien.HoTen} (ID: {nhanVien.MaNV}), gán MaTK: {nhanVien.MaTK ?? "N/A"}", "Info");

                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Thêm nhân viên mới thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
            LoadUserData(null, nhanVien.MaTK);
            return View(nhanVien);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            LoadUserData(id, nhanVien.MaTK);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebAppBookingBoat.Models.NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV) return NotFound();

            ModelState.Remove("AppUser");
            ModelState.Remove("HoaDons");

            if (string.IsNullOrWhiteSpace(nhanVien.MaTK)) nhanVien.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckNhanVienLogic(nhanVien, id);
                if (error == null)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Update(nhanVien);
                        await _context.SaveChangesAsync();

                        await GhiLogHeThong("Cập nhật nhân viên", "NhanViens",
                            $"Cập nhật thông tin nhân viên: {nhanVien.HoTen} (ID: {id})", "Info");

                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }

            LoadUserData(id, nhanVien.MaTK);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanViens.Include(n => n.AppUser).FirstOrDefaultAsync(n => n.MaNV == id);
            if (nv == null) return Json(new { success = false, message = "Không tìm thấy nhân viên." });

            bool hasInvoices = await _context.HoaDons.AnyAsync(h => h.MaNV == id);

            try
            {
                if (hasInvoices)
                {
                    nv.TrangThai = false; // Nghỉ việc
                    if (nv.AppUser != null)
                    {
                        nv.AppUser.TrangThai = false; // Khóa tài khoản đăng nhập
                        _context.Update(nv.AppUser);
                    }
                    _context.Update(nv);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Khóa nhân viên", "NhanViens", $"Nhân viên {nv.HoTen} đã nghỉ việc (Soft Delete)", "Warning");
                    return Json(new { success = true, message = "Đã cập nhật trạng thái nghỉ việc và khóa tài khoản." });
                }

                _context.NhanViens.Remove(nv);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Xóa nhân viên", "NhanViens", $"Xóa vĩnh viễn nhân viên: {nv.HoTen}", "Warning");
                return Json(new { success = true, message = "Đã xóa nhân viên vĩnh viễn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #region Helpers

        [NonAction]
        private async Task GhiLogHeThong(string hanhDong, string bang, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = bang,
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}