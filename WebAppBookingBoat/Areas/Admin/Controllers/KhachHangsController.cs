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

        // Hàm ghi log tập trung
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

        private async Task<string?> CheckUpsertLogic(KhachHang kh, int? id = null)
        {
            if (await _context.KhachHangs.AnyAsync(k => k.Email == kh.Email && k.MaKH != id))
                return "Email này đã tồn tại trong hệ thống!";

            if (await _context.KhachHangs.AnyAsync(k => k.Sdt == kh.Sdt && k.MaKH != id))
                return "Số điện thoại này đã được sử dụng!";

            if (kh.MaTK != null && await _context.KhachHangs.AnyAsync(k => k.MaTK == kh.MaTK && k.MaKH != id))
                return "Tài khoản này đã được gán cho khách hàng khác!";

            return null;
        }

        private async Task<(bool canExecute, bool isHardDelete, string message)> CheckDeleteLogic(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return (false, false, "Khách hàng không tồn tại.");

            bool daCoHoaDon = await _context.HoaDons.AnyAsync(h => h.MaKH == id);
            if (daCoHoaDon)
            {
                return (true, false, "Khách hàng đã có lịch sử hóa đơn. Hệ thống sẽ khóa tài khoản thay vì xóa.");
            }

            return (true, true, "");
        }

        private void LoadUserData(int? currentMaKH = null, string? selectedMaTK = null)
        {
            var assignedUserIds = _context.KhachHangs
                .Where(k => k.MaTK != null && k.MaKH != currentMaKH)
                .Select(k => k.MaTK).ToList();

            var availableUsers = _context.Users
                .Where(u => !assignedUserIds.Contains(u.Id)).ToList();

            ViewBag.MaTK = new SelectList(availableUsers, "Id", "UserName", selectedMaTK);
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

                    await GhiLogHeThong("Thêm khách hàng", $"Tạo mới KH: {khachHang.HoTen} (ID: {khachHang.MaKH}). Email: {khachHang.Email}");

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

            var oldData = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.MaKH == id);
            if (oldData == null) return NotFound();

            if (ModelState.IsValid)
            {
                var error = await CheckUpsertLogic(khachHang, id);
                if (error == null)
                {
                    try
                    {
                        _context.Update(khachHang);
                        await _context.SaveChangesAsync();

                        // Log chi tiết nếu thay đổi thông tin quan trọng
                        string detail = $"Cập nhật KH ID: {id}.";
                        if (oldData.Email != khachHang.Email) detail += $" Đổi Email: {oldData.Email} -> {khachHang.Email}.";
                        if (oldData.MaTK != khachHang.MaTK) detail += $" Thay đổi liên kết tài khoản.";

                        await GhiLogHeThong("Cập nhật khách hàng", detail);

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

                await GhiLogHeThong("Xóa khách hàng", $"Xóa vĩnh viễn KH: {kh.HoTen} (ID: {id})", "Warning");

                return Json(new { success = true, message = "Đã xóa khách hàng vĩnh viễn." });
            }
            else
            {
                if (kh.AppUser != null)
                {
                    kh.AppUser.TrangThai = false;
                    _context.Update(kh.AppUser);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Khóa tài khoản khách hàng", $"Khóa tài khoản của KH: {kh.HoTen} do đã có hóa đơn.", "Warning");

                    return Json(new { success = true, message = "Đã khóa tài khoản khách hàng để giữ lịch sử hóa đơn." });
                }
                return Json(new { success = false, message = "Không thể khóa vì khách hàng không có tài khoản liên kết." });
            }
        }

        #endregion
    }
}