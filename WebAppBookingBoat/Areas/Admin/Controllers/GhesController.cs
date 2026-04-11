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

        public GhesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? maTau)
        {
            var query = _context.Ghes.Include(g => g.Tau).AsQueryable();

            if (maTau.HasValue)
            {
                query = query.Where(g => g.MaTau == maTau);
                ViewBag.CurrentMaTau = maTau;
            }

            var ghes = await query.OrderBy(g => g.MaTau).ThenBy(g => g.TenGhe).ToListAsync();

            // Đổ danh sách tàu vào dropdown để lọc và để Modal dùng
            ViewBag.MaTau = new SelectList(_context.Taus, "MaTau", "TenTau", maTau);

            return View(ghes);
        }

        // 1. Hàm logic kiểm tra điều kiện xóa
        private async Task<(bool canDelete, string message)> CheckCanDeleteGhe(int id)
        {
            var ghe = await _context.Ghes.FindAsync(id);
            if (ghe == null) return (false, "Ghế không tồn tại.");

            // Kiểm tra vé đã đặt
            bool daCoVe = await _context.Ves.AnyAsync(v => v.MaGhe == id);
            if (daCoVe)
            {
                return (false, $"Ghế {ghe.TenGhe} đã được bán vé, không thể xóa!");
            }

            return (true, "");
        }

        // 2. AJAX: Lấy thông tin tàu để Modal hiển thị "Còn trống bao nhiêu"
        [HttpGet]
        public async Task<JsonResult> GetBoatInfo(int id)
        {
            var tau = await _context.Taus
                .Select(t => new
                {
                    t.MaTau,
                    t.TenTau,
                    t.TongSoGhe,
                    CurrentCount = t.Ghes.Count // Đếm tổng số ghế đang có trong DB
                })
                .FirstOrDefaultAsync(t => t.MaTau == id);
            return Json(tau);
        }

        // 3. LOGIC QUAN TRỌNG: SINH GHẾ HÀNG LOẠT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoGenerate(int MaTau, int SoLuong, string LoaiGhe)
        {
            var tau = await _context.Taus
                .Include(t => t.Ghes)
                .FirstOrDefaultAsync(t => t.MaTau == MaTau);

            if (tau == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tàu!";
                return RedirectToAction(nameof(Index));
            }

            // KIỂM TRA TỔNG SỨC CHỨA
            int tongHienTai = tau.Ghes.Count();
            if (tongHienTai + SoLuong > tau.TongSoGhe)
            {
                TempData["Error"] = $"Không thể thêm! Tàu {tau.TenTau} chỉ còn trống {tau.TongSoGhe - tongHienTai} chỗ.";
                return RedirectToAction(nameof(Index), new { maTau = MaTau });
            }

            // XÁC ĐỊNH TIỀN TỐ (V- cho VIP, T- cho Thường)
            string prefix = (LoaiGhe == "VIP") ? "V-" : "T-";

            // Lấy số thứ tự lớn nhất hiện có của LOẠI GHẾ ĐÓ trên tàu này để sinh tiếp nối
            // Ví dụ: Đã có T-10 thì sinh tiếp T-11
            int maxStt = 0;
            var ghesCungLoai = tau.Ghes.Where(g => g.LoaiGhe == LoaiGhe).ToList();
            if (ghesCungLoai.Any())
            {
                // Parse số từ TenGhe (ví dụ "T-10" lấy ra 10)
                maxStt = ghesCungLoai.Max(g =>
                {
                    int num;
                    return int.TryParse(g.TenGhe.Replace(prefix, ""), out num) ? num : 0;
                });
            }

            for (int i = 1; i <= SoLuong; i++)
            {
                _context.Ghes.Add(new Ghe
                {
                    MaTau = MaTau,
                    TenGhe = $"{prefix}{(maxStt + i):D2}", // Định dạng 2 chữ số (01, 02...)
                    LoaiGhe = LoaiGhe
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã sinh thành công {SoLuong} ghế {LoaiGhe} cho tàu {tau.TenTau}.";
            return RedirectToAction(nameof(Index), new { maTau = MaTau });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAjax(int id)
        {
            // Gọi hàm logic kiểm tra
            var (canDelete, message) = await CheckCanDeleteGhe(id);

            if (!canDelete)
            {
                return Json(new { success = false, message = message });
            }

            try
            {
                var ghe = await _context.Ghes.FindAsync(id);
                _context.Ghes.Remove(ghe!);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa ghế thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa dữ liệu." });
            }
        }

        // 5. CÁC HÀM CƠ BẢN KHÁC
        public IActionResult Create()
        {
            ViewData["MaTau"] = new SelectList(_context.Taus, "MaTau", "TenTau");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTau,TenGhe,LoaiGhe")] Ghe ghe)
        {
            // Loại bỏ kiểm tra validate cho thuộc tính Tau (vì form chỉ gửi MaTau)
            ModelState.Remove("Tau");
            if (ModelState.IsValid)
            {
                _context.Add(ghe);
                await _context.SaveChangesAsync();
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
            // Loại bỏ kiểm tra validate cho thuộc tính Tau (vì form chỉ gửi MaTau)
            ModelState.Remove("Tau");
            if (id != ghe.MaGhe) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(ghe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index), new { maTau = ghe.MaTau });
            }
            return View(ghe);
        }
    }
}