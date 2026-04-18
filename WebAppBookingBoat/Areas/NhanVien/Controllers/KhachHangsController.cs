using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "Staff, Nhân viên, Admin")] // Cho phép nhân viên bán hàng truy cập
    public class KhachHangsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public KhachHangsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region HELPERS

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

        private async Task<string?> CheckCreateLogic(WebAppBookingBoat.Models.KhachHang kh)
        {
            if (await _context.KhachHangs.AnyAsync(k => k.Email == kh.Email))
                return "Email này đã tồn tại trên hệ thống!";

            if (await _context.KhachHangs.AnyAsync(k => k.Sdt == kh.Sdt))
                return "Số điện thoại này đã được sử dụng!";

            if (kh.MaTK != null)
            {
                if (await _context.KhachHangs.AnyAsync(k => k.MaTK == kh.MaTK))
                    return "Tài khoản này đã được gán cho khách hàng khác!";

                if (await _context.NhanViens.AnyAsync(nv => nv.MaTK == kh.MaTK))
                    return "Tài khoản này thuộc về một Nhân viên!";
            }
            return null;
        }

        private void LoadUserData(string? selectedMaTK = null)
        {
            var assignedInCustomer = _context.KhachHangs.Where(k => k.MaTK != null).Select(k => k.MaTK);
            var assignedInStaff = _context.NhanViens.Where(n => n.MaTK != null).Select(n => n.MaTK);
            var allAssignedIds = assignedInCustomer.Union(assignedInStaff).ToList();

            var availableUsers = _context.Users
                .Where(u => !allAssignedIds.Contains(u.Id))
                .Select(u => new { u.Id, Display = u.UserName + " (" + u.Email + ")" })
                .ToList();

            ViewBag.MaTK = new SelectList(availableUsers, "Id", "Display", selectedMaTK);
        }

        #endregion

        #region ACTIONS

        // 1. Xem danh sách khách hàng
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

        // 2. Xem chi tiết khách hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var kh = await _context.KhachHangs.Include(k => k.AppUser).FirstOrDefaultAsync(m => m.MaKH == id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        // 3. Tạo mới khách hàng (Dành cho khách mua tại quầy chưa có hồ sơ)
        public IActionResult Create()
        {
            LoadUserData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebAppBookingBoat.Models.KhachHang khachHang)
        {
            ModelState.Remove("AppUser");
            ModelState.Remove("HoaDons");

            if (string.IsNullOrWhiteSpace(khachHang.MaTK)) khachHang.MaTK = null;

            if (ModelState.IsValid)
            {
                var error = await CheckCreateLogic(khachHang);
                if (error == null)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Add(khachHang);
                        await _context.SaveChangesAsync();

                        await GhiLogHeThong("NV thêm khách", $"Nhân viên {User.Identity?.Name} tạo hồ sơ KH: {khachHang.HoTen}");

                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        if (transaction.GetDbTransaction().Connection != null) await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
            LoadUserData(khachHang.MaTK);
            return View(khachHang);
        }

        #endregion
    }
}