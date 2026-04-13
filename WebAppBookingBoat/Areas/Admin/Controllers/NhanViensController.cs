using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
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

            if (await _context.NhanViens.AnyAsync(n => n.Email == nv.Email && n.MaNV != id))
                return "Email này đã tồn tại trong hệ thống!";

            if (await _context.NhanViens.AnyAsync(n => n.Sdt == nv.Sdt && n.MaNV != id))
                return "Số điện thoại này đã tồn tại!";

            if (!string.IsNullOrEmpty(nv.MaTK) && await _context.NhanViens.AnyAsync(n => n.MaTK == nv.MaTK && n.MaNV != id))
                return "Tài khoản này đã được gán cho nhân viên khác!";

            return null;
        }

        private void LoadUserData(int? currentMaNV = null, string? selectedMaTK = null)
        {
            var assignedUserIds = _context.NhanViens
                .Where(n => n.MaTK != null && n.MaNV != currentMaNV)
                .Select(n => n.MaTK).ToList();

            var availableUsers = _context.Users
                .Where(u => !assignedUserIds.Contains(u.Id)).ToList();

            ViewData["MaTK"] = new SelectList(availableUsers, "Id", "UserName", selectedMaTK);
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

            var nhanVien = await _context.NhanViens
                .Include(n => n.AppUser)
                .FirstOrDefaultAsync(m => m.MaNV == id);

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

            if (string.IsNullOrWhiteSpace(nhanVien.MaTK)) nhanVien.MaTK = null!;

            if (ModelState.IsValid)
            {
                var error = await CheckNhanVienLogic(nhanVien);
                if (error == null)
                {
                    _context.Add(nhanVien);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Thêm nhân viên", "NhanViens", $"Thêm mới nhân viên: {nhanVien.HoTen} (ID: {nhanVien.MaNV})");

                    TempData["SuccessMessage"] = "Thêm nhân viên mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error);
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
                    try
                    {
                        _context.Update(nhanVien);
                        await _context.SaveChangesAsync();

                        await GhiLogHeThong("Cập nhật nhân viên", "NhanViens", $"Cập nhật thông tin nhân viên: {nhanVien.HoTen} (ID: {id})");

                        TempData["SuccessMessage"] = "Cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.NhanViens.Any(e => e.MaNV == nhanVien.MaNV)) return NotFound();
                        throw;
                    }
                }
                ModelState.AddModelError("", error);
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
                    // Khóa mềm (Soft Delete)
                    nv.TrangThai = false;
                    string note = "Khóa tài khoản và cập nhật trạng thái nghỉ việc (do đã có hóa đơn)";

                    if (nv.AppUser != null)
                    {
                        nv.AppUser.TrangThai = false;
                        _context.Update(nv.AppUser);
                        note += " kèm khóa tài khoản User.";
                    }

                    _context.Update(nv);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Khóa nhân viên", "NhanViens", $"Nhân viên: {nv.HoTen} (ID: {id}). Lý do: {note}", "Warning");

                    return Json(new { success = true, message = "Nhân viên đã có hóa đơn. Hệ thống đã khóa tài khoản và cập nhật trạng thái nghỉ việc." });
                }

                // Xóa vĩnh viễn (Hard Delete)
                string tenNV = nv.HoTen;
                _context.NhanViens.Remove(nv);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Xóa nhân viên", "NhanViens", $"Đã xóa vĩnh viễn nhân viên: {tenNV} (ID: {id})", "Warning");

                return Json(new { success = true, message = "Đã xóa nhân viên vĩnh viễn khỏi hệ thống." });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi khi xóa/khóa nhân viên", "NhanViens", $"ID: {id}. Lỗi: {ex.Message}", "Error");
                return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình xử lý." });
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