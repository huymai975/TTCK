using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GhesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public GhesController(ApplicationDbContext context, UserManager<AppUser> userManager)
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
                BangTacDong = "Ghes",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        private async Task<(bool isBusy, string message)> CheckBoatIsBusy(int maTau)
        {
            var lichTrinhDangChay = await _context.LichTrinhs
                .AnyAsync(l => l.MaTau == maTau &&
                               l.TrangThai != "Hoàn thành" &&
                               l.TrangThai != "Đã hủy");

            if (lichTrinhDangChay)
            {
                return (true, "Tàu này hiện đang có lịch trình sắp khởi hành hoặc đang vận hành. Không thể thay đổi cấu hình ghế!");
            }
            return (false, "");
        }

        private async Task<(bool canDelete, string message)> CheckCanDeleteGhe(int id)
        {
            var ghe = await _context.Ghes
                .Include(g => g.Ves).ThenInclude(v => v.HoaDon)
                .FirstOrDefaultAsync(g => g.MaGhe == id);

            if (ghe == null) return (false, "Ghế không tồn tại.");

            var veViPham = await _context.Ves
                .Include(v => v.HoaDon)
                .Where(v => v.MaGhe == id && v.HoaDon!.TrangThai != "Đã hủy")
                .FirstOrDefaultAsync();

            if (veViPham != null)
            {
                return (false, $"Ghế {ghe.TenGhe} đang nằm trong Hóa đơn #{veViPham.MaHoaDon}. Không thể xóa!");
            }

            bool daTungCoLichSu = await _context.Ves.AnyAsync(v => v.MaGhe == id);
            if (daTungCoLichSu)
            {
                return (false, $"Ghế {ghe.TenGhe} đã có lịch sử vé. Để đảm bảo báo cáo chính xác, bạn không nên xóa ghế này.");
            }

            return (true, "");
        }

        #endregion

        #region ACTION METHODS

        public async Task<IActionResult> Index(int? maTau)
        {
            var query = _context.Ghes.Include(g => g.Tau).AsQueryable();

            if (maTau.HasValue)
            {
                query = query.Where(g => g.MaTau == maTau);
                ViewBag.CurrentMaTau = maTau;
            }

            var ghes = await query.OrderBy(g => g.MaTau).ThenBy(g => g.TenGhe).ToListAsync();
            ViewBag.MaTau = new SelectList(_context.Taus, "MaTau", "TenTau", maTau);

            return View(ghes);
        }

        [HttpGet]
        public async Task<JsonResult> GetBoatInfo(int id)
        {
            var tau = await _context.Taus
                .Select(t => new { t.MaTau, t.TenTau, t.TongSoGhe, CurrentCount = t.Ghes.Count })
                .FirstOrDefaultAsync(t => t.MaTau == id);
            return Json(tau);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoGenerate(int MaTau, int SoLuong, string LoaiGhe)
        {
            var tau = await _context.Taus.Include(t => t.Ghes).FirstOrDefaultAsync(t => t.MaTau == MaTau);
            if (tau == null) return RedirectToAction(nameof(Index));

            int tongHienTai = tau.Ghes.Count();
            if (tongHienTai + SoLuong > tau.TongSoGhe)
            {
                TempData["Error"] = $"Không thể thêm! Tàu {tau.TenTau} chỉ còn trống {tau.TongSoGhe - tongHienTai} chỗ.";
                return RedirectToAction(nameof(Index), new { maTau = MaTau });
            }

            string prefix = (LoaiGhe == "VIP") ? "V-" : "T-";
            int maxStt = 0;
            var ghesCungLoai = tau.Ghes.Where(g => g.LoaiGhe == LoaiGhe).ToList();
            if (ghesCungLoai.Any())
            {
                maxStt = ghesCungLoai.Max(g =>
                {
                    int num;
                    return int.TryParse(g.TenGhe.Replace(prefix, ""), out num) ? num : 0;
                });
            }

            for (int i = 1; i <= SoLuong; i++)
            {
                _context.Ghes.Add(new Ghe { MaTau = MaTau, TenGhe = $"{prefix}{(maxStt + i):D2}", LoaiGhe = LoaiGhe });
            }

            await _context.SaveChangesAsync();

            // Ghi Log: Sinh ghế tự động
            await GhiLogHeThong("Sinh ghế tự động", $"Đã tạo {SoLuong} ghế {LoaiGhe} cho tàu {tau.TenTau} (ID: {MaTau}).");

            TempData["Success"] = $"Đã sinh thành công {SoLuong} ghế {LoaiGhe} cho tàu {tau.TenTau}.";
            return RedirectToAction(nameof(Index), new { maTau = MaTau });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAjax(int id)
        {
            var ghe = await _context.Ghes.Include(g => g.Tau).FirstOrDefaultAsync(g => g.MaGhe == id);
            if (ghe == null) return Json(new { success = false, message = "Ghế không tồn tại." });

            var (isBusy, busyMessage) = await CheckBoatIsBusy(ghe.MaTau);
            if (isBusy) return Json(new { success = false, message = busyMessage });

            var (canDelete, deleteMessage) = await CheckCanDeleteGhe(id);
            if (!canDelete) return Json(new { success = false, message = deleteMessage });

            try
            {
                _context.Ghes.Remove(ghe);
                await _context.SaveChangesAsync();

                // Ghi Log: Xóa ghế
                await GhiLogHeThong("Xóa ghế", $"Xóa ghế {ghe.TenGhe} của tàu {ghe.Tau?.TenTau} (ID Ghế: {id})", "Warning");

                return Json(new { success = true, message = "Đã xóa ghế thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public IActionResult Create()
        {
            ViewData["MaTau"] = new SelectList(_context.Taus, "MaTau", "TenTau");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTau,TenGhe,LoaiGhe")] Ghe ghe)
        {
            ModelState.Remove("Tau");
            if (ModelState.IsValid)
            {
                _context.Add(ghe);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Thêm ghế lẻ", $"Tạo ghế {ghe.TenGhe} cho MaTau: {ghe.MaTau}");

                TempData["SuccessMessage"] = "Thêm mới ghế thành công!";
                return RedirectToAction(nameof(Index), new { maTau = ghe.MaTau });
            }
            ViewData["MaTau"] = new SelectList(_context.Taus, "MaTau", "TenTau", ghe.MaTau);
            return View(ghe);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ghe = await _context.Ghes.FindAsync(id);
            if (ghe == null) return NotFound();
            ViewData["MaTau"] = new SelectList(_context.Taus, "MaTau", "TenTau", ghe.MaTau);
            return View(ghe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaGhe,MaTau,TenGhe,LoaiGhe")] Ghe ghe)
        {
            ModelState.Remove("Tau");
            if (id != ghe.MaGhe) return NotFound();

            if (ModelState.IsValid)
            {
                var (isBusy, busyMessage) = await CheckBoatIsBusy(ghe.MaTau);
                if (isBusy)
                {
                    TempData["Error"] = busyMessage;
                    return RedirectToAction(nameof(Index), new { maTau = ghe.MaTau });
                }

                try
                {
                    _context.Update(ghe);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật ghế", $"Sửa thông tin ghế ID: {id}. Tên mới: {ghe.TenGhe}");

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index), new { maTau = ghe.MaTau });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ghes.Any(e => e.MaGhe == ghe.MaGhe)) return NotFound();
                    else throw;
                }
            }
            ViewData["MaTau"] = new SelectList(_context.Taus, "MaTau", "TenTau", ghe.MaTau);
            return View(ghe);
        }

        #endregion
    }
}