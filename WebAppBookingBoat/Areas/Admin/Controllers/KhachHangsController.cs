using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;

        public KhachHangsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region PRIVATE LOGIC & HELPERS

        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = "KhachHangs",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(null);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(null);

            return Json(new
            {
                email = user.Email,
                phoneNumber = user.PhoneNumber
            });
        }

        private async Task<string?> CheckUpsertLogic(WebAppBookingBoat.Models.KhachHang kh, int? id = null)
        {
            if (await _context.KhachHangs.AnyAsync(k => k.Email == kh.Email && k.MaKH != id))
                return "Email này đã tồn tại trong hệ thống!";

            if (await _context.KhachHangs.AnyAsync(k => k.Sdt == kh.Sdt && k.MaKH != id))
                return "Số điện thoại này đã được sử dụng!";

            if (kh.MaTK != null)
            {
                // Kiểm tra xem đã gán cho khách hàng khác chưa
                if (await _context.KhachHangs.AnyAsync(k => k.MaTK == kh.MaTK && k.MaKH != id))
                    return "Tài khoản này đã được gán cho khách hàng khác!";

                // KIỂM TRA CHÉO: Tài khoản này có phải là nhân viên không?
                if (await _context.NhanViens.AnyAsync(nv => nv.MaTK == kh.MaTK))
                    return "Tài khoản này đã được gán cho một Nhân viên, không thể làm Khách hàng!";
            }

            return null;
        }

        private void LoadUserData(int? currentMaKH = null, string? selectedMaTK = null)
        {
            // 1. Lấy ID đã gán ở bảng Khách hàng (trừ chính mình nếu đang sửa)
            var assignedInCustomer = _context.KhachHangs
                .Where(k => k.MaTK != null && k.MaKH != currentMaKH)
                .Select(k => k.MaTK);

            // 2. Lấy ID đã gán ở bảng Nhân viên (Lọc chéo sạch 100%)
            var assignedInStaff = _context.NhanViens
                .Where(n => n.MaTK != null)
                .Select(n => n.MaTK);

            // 3. Hợp nhất danh sách đen
            var allAssignedIds = assignedInCustomer.Union(assignedInStaff).ToList();

            // 4. Lấy danh sách sạch
            var availableUsers = _context.Users
                .Where(u => !allAssignedIds.Contains(u.Id))
                .Select(u => new { u.Id, Display = u.UserName + " (" + u.Email + ")" })
                .ToList();

            ViewBag.MaTK = new SelectList(availableUsers, "Id", "Display", selectedMaTK);
        }

        #endregion

        #region ACTION METHODS

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
        public async Task<IActionResult> Create(WebAppBookingBoat.Models.KhachHang khachHang)
        {
            // Xóa các navigation property khỏi validation
            ModelState.Remove("AppUser");
            ModelState.Remove("HoaDons");

            if (string.IsNullOrWhiteSpace(khachHang.MaTK)) khachHang.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckUpsertLogic(khachHang);
                if (error == null)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Add(khachHang);
                        await _context.SaveChangesAsync();

                        await GhiLogHeThong("Thêm khách hàng", $"Tạo mới KH: {khachHang.HoTen} (ID: {khachHang.MaKH}). Tài khoản: {khachHang.MaTK ?? "Khách vãng lai"}");

                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
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
        public async Task<IActionResult> Edit(int id, WebAppBookingBoat.Models.KhachHang khachHang)
        {
            if (id != khachHang.MaKH) return NotFound();

            ModelState.Remove("AppUser");
            ModelState.Remove("HoaDons");

            if (string.IsNullOrWhiteSpace(khachHang.MaTK)) khachHang.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckUpsertLogic(khachHang, id);
                if (error == null)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Update(khachHang);
                        await _context.SaveChangesAsync();

                        await GhiLogHeThong("Cập nhật khách hàng", $"Sửa KH ID: {id}. HoTen: {khachHang.HoTen}");

                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
            LoadUserData(id, khachHang.MaTK);
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var kh = await _context.KhachHangs.Include(k => k.AppUser).FirstOrDefaultAsync(m => m.MaKH == id);
            if (kh == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

            bool daCoHoaDon = await _context.HoaDons.AnyAsync(h => h.MaKH == id);

            try
            {
                if (daCoHoaDon)
                {
                    // Soft delete - Khóa tài khoản
                    if (kh.AppUser != null)
                    {
                        kh.AppUser.TrangThai = false;
                        _context.Update(kh.AppUser);
                        await _context.SaveChangesAsync();
                        await GhiLogHeThong("Khóa khách hàng", $"Khóa tài khoản KH: {kh.HoTen} do đã có hóa đơn.", "Warning");
                        return Json(new { success = true, message = "Khách hàng đã có hóa đơn. Hệ thống đã khóa tài khoản liên kết." });
                    }
                    return Json(new { success = false, message = "Khách hàng này đã có hóa đơn và không có tài khoản để khóa. Không thể xóa vĩnh viễn." });
                }

                // Hard delete
                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();
                await GhiLogHeThong("Xóa khách hàng", $"Xóa vĩnh viễn KH: {kh.HoTen}", "Warning");

                return Json(new { success = true, message = "Đã xóa khách hàng vĩnh viễn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        #endregion
    }
}