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

        // --- HÀM LOGIC TẬP TRUNG ---
        private async Task<string?> CheckBusinessLogic(KhachHang kh, int? id = null)
        {
            // Kiểm tra trùng Email
            if (await _context.KhachHangs.AnyAsync(k => k.Email == kh.Email && k.MaKH != id))
                return "Email này đã tồn tại trong hệ thống!";

            // Kiểm tra trùng Số điện thoại (nếu cần)
            if (await _context.KhachHangs.AnyAsync(k => k.Sdt == kh.Sdt && k.MaKH != id))
                return "ố điện thoại này đã được sử dụng!";

            // Kiểm tra trùng Tài khoản (MaTK)
            if (kh.MaTK != null && await _context.KhachHangs.AnyAsync(k => k.MaTK == kh.MaTK && k.MaKH != id))
                return "Tài khoản này đã được gán cho khách hàng khác!";

            return null; // Không có lỗi
        }

        // --- HÀM LOAD DROPDOWN TẬP TRUNG ---
        private void LoadUserData(int? currentMaKH = null, string? selectedMaTK = null)
        {
            var assignedUserIds = _context.KhachHangs
                .Where(k => k.MaTK != null && k.MaKH != currentMaKH)
                .Select(k => k.MaTK).ToList();

            var availableUsers = _context.Users
                .Where(u => !assignedUserIds.Contains(u.Id)).ToList();

            ViewBag.MaTK = new SelectList(availableUsers, "Id", "UserName", selectedMaTK);
        }

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
                var error = await CheckBusinessLogic(khachHang);
                if (error == null)
                {
                    try
                    {
                        _context.Add(khachHang);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đăng ký khách hàng mới thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch { error = "Lỗi hệ thống: Không thể lưu dữ liệu."; }
                }
                TempData["ErrorMessage"] = error; // Hiện SweetAlert cho lỗi ràng buộc/hệ thống
            }

            LoadUserData(null, khachHang.MaTK);
            return View(khachHang);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null) return NotFound();

            LoadUserData(id, khachHang.MaTK);
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKH,MaTK,HoTen,NgaySinh,Sdt,Email,DiaChi")] KhachHang khachHang)
        {
            if (id != khachHang.MaKH) return NotFound();
            if (string.IsNullOrWhiteSpace(khachHang.MaTK)) khachHang.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckBusinessLogic(khachHang, id);
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
                    catch { error = "Lỗi hệ thống: Không thể cập nhật."; }
                }
                TempData["ErrorMessage"] = error;
            }

            LoadUserData(id, khachHang.MaTK);
            return View(khachHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null) return RedirectToAction(nameof(Index));

            if (await _context.HoaDons.AnyAsync(h => h.MaKH == id))
            {
                TempData["ErrorMessage"] = "Không thể xóa vì khách hàng này đã có hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            _context.KhachHangs.Remove(khachHang);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa hồ sơ khách hàng thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}