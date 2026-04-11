using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanViensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanViensController(ApplicationDbContext context)
        {
            _context = context;
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

        // --- BỔ SUNG ACTION DETAILS ---
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

            if (string.IsNullOrWhiteSpace(nhanVien.MaTK))
            {
                nhanVien.MaTK = null; // Đảm bảo truyền null thực sự nếu không có tài khoản
            }

            if (ModelState.IsValid)
            {
                var error = await CheckNhanVienLogic(nhanVien, id);
                if (error == null)
                {
                    try
                    {
                        // 2. TRÁNH LỖI THEO DÕI (TRACKING):
                        // Nếu DBContext đang theo dõi một instance khác của NhanVien, 
                        // hãy dùng cách này để update an toàn hơn
                        _context.Update(nhanVien);
                        await _context.SaveChangesAsync();

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

            // Nếu đến đây là có lỗi, load lại dữ liệu cho Dropdown
            LoadUserData(id, nhanVien.MaTK);
            return View(nhanVien);
        }

        // --- CẬP NHẬT DELETE AJAX ĐỒNG NHẤT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanViens.Include(n => n.AppUser).FirstOrDefaultAsync(n => n.MaNV == id);
            if (nv == null) return Json(new { success = false, message = "Không tìm thấy nhân viên." });

            bool hasInvoices = await _context.HoaDons.AnyAsync(h => h.MaNV == id);

            if (hasInvoices)
            {
                // Nếu đã có hóa đơn -> Khóa mềm (Soft Delete)
                nv.TrangThai = false;
                if (nv.AppUser != null)
                {
                    nv.AppUser.TrangThai = false;
                    _context.Update(nv.AppUser);
                }
                _context.Update(nv);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Nhân viên đã có hóa đơn. Hệ thống đã khóa tài khoản và cập nhật trạng thái nghỉ việc." });
            }

            // Nếu chưa có hóa đơn -> Xóa vĩnh viễn (Hard Delete)
            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa nhân viên vĩnh viễn khỏi hệ thống." });
        }
    }
}