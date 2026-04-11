using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhachHangsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhachHangsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. HÀM LOGIC CHO THÊM & SỬA ---
        private async Task<string?> CheckUpsertLogic(KhachHang kh, int? id = null)
        {
            // Kiểm tra trùng Email
            if (await _context.KhachHangs.AnyAsync(k => k.Email == kh.Email && k.MaKH != id))
                return "Email này đã tồn tại trong hệ thống!";

            // Kiểm tra trùng Số điện thoại
            if (await _context.KhachHangs.AnyAsync(k => k.Sdt == kh.Sdt && k.MaKH != id))
                return "Số điện thoại này đã được sử dụng!";

            // Kiểm tra trùng Tài khoản (MaTK)
            if (kh.MaTK != null && await _context.KhachHangs.AnyAsync(k => k.MaTK == kh.MaTK && k.MaKH != id))
                return "Tài khoản này đã được gán cho khách hàng khác!";

            return null; // OK
        }

        // --- 2. HÀM LOGIC CHO XÓA ---
        private async Task<(bool canDelete, bool isHardDelete, string message)> CheckDeleteLogic(int id)
        {
            var kh = await _context.KhachHangs.Include(k => k.AppUser).FirstOrDefaultAsync(m => m.MaKH == id);
            if (kh == null) return (false, false, "Khách hàng không tồn tại.");

            bool daCoHoaDon = await _context.HoaDons.AnyAsync(h => h.MaKH == id);

            if (daCoHoaDon)
            {
                // Trả về false cho canDelete nhưng đánh dấu đây là trường hợp chuyển sang khóa tài khoản
                return (true, false, "Khách hàng đã có lịch sử hóa đơn. Hệ thống sẽ khóa tài khoản thay vì xóa.");
            }

            return (true, true, "");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var (canExecute, isHardDelete, message) = await CheckDeleteLogic(id);
            if (!canExecute) return Json(new { success = false, message = message });

            var kh = await _context.KhachHangs.Include(k => k.AppUser).FirstOrDefaultAsync(m => m.MaKH == id);
            if (kh == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            if (isHardDelete)
            {
                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa khách hàng vĩnh viễn." });
            }
            else
            {
                // Khóa tài khoản nếu có hóa đơn
                if (kh.AppUser != null)
                {
                    kh.AppUser.TrangThai = false;
                    _context.Update(kh.AppUser);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã khóa tài khoản khách hàng để giữ lịch sử hóa đơn." });
                }
                return Json(new { success = false, message = "Không thể khóa vì khách hàng không có tài khoản liên kết." });
            }
        }

        // --- 3. HÀM LOAD DROPDOWN TẬP TRUNG ---
        private void LoadUserData(int? currentMaKH = null, string? selectedMaTK = null)
        {
            var assignedUserIds = _context.KhachHangs
                .Where(k => k.MaTK != null && k.MaKH != currentMaKH)
                .Select(k => k.MaTK).ToList();

            var availableUsers = _context.Users
                .Where(u => !assignedUserIds.Contains(u.Id)).ToList();

            ViewBag.MaTK = new SelectList(availableUsers, "Id", "UserName", selectedMaTK);
        }

        // ================= ACTION METHODS =================

        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.KhachHangs.Include(k => k.AppUser).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(k => k.HoTen.Contains(searchString) || k.Sdt.Contains(searchString));
                ViewBag.Search = searchString;
            }
            return View(await query.OrderByDescending(k => k.MaKH).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var kh = await _context.KhachHangs.Include(k => k.AppUser).FirstOrDefaultAsync(m => m.MaKH == id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        public IActionResult Create()
        {
            LoadUserData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTK,HoTen,NgaySinh,Sdt,Email,DiaChi")] KhachHang khachHang)
        {
            if (string.IsNullOrWhiteSpace(khachHang.MaTK)) khachHang.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckUpsertLogic(khachHang);
                if (error == null)
                {
                    _context.Add(khachHang);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = error;
            }
            LoadUserData(null, khachHang.MaTK);
            return View(khachHang);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound();
            LoadUserData(id, kh.MaTK);
            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKH,MaTK,HoTen,NgaySinh,Sdt,Email,DiaChi")] KhachHang khachHang)
        {
            if (id != khachHang.MaKH) return NotFound();
            if (string.IsNullOrWhiteSpace(khachHang.MaTK)) khachHang.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckUpsertLogic(khachHang, id);
                if (error == null)
                {
                    try
                    {
                        _context.Update(khachHang);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException) { error = "Dữ liệu đã bị thay đổi, hãy tải lại trang."; }
                }
                TempData["ErrorMessage"] = error;
            }
            LoadUserData(id, khachHang.MaTK);
            return View(khachHang);
        }
    }
}