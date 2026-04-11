using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VesController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region LOGIC TẬP TRUNG (Business Rules)

        private async Task ValidateVeBusiness(Ve ve, bool isEdit = false, string? trangThaiCu = null)
        {
            // 1. Kiểm tra lịch trình tồn tại
            var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);
            if (lichTrinh == null)
            {
                ModelState.AddModelError("MaLichTrinh", "Lịch trình không tồn tại.");
                return;
            }

            // 2. Kiểm tra trùng ghế (Trừ trường hợp vé đã hủy)
            bool biTrungGhe = await _context.Ves.AnyAsync(v =>
                v.MaLichTrinh == ve.MaLichTrinh &&
                v.MaGhe == ve.MaGhe &&
                v.MaVe != ve.MaVe &&
                v.TrangThai != "Đã hủy");

            if (ve.TrangThai != "Đã hủy" && biTrungGhe)
            {
                ModelState.AddModelError("MaGhe", "Ghế này đã được đặt bởi một vé khác.");
            }

            // 3. Kiểm tra số lượng ghế trống khi tạo mới hoặc chuyển từ Hủy -> Hợp lệ
            if (ve.TrangThai != "Đã hủy" && (trangThaiCu == "Đã hủy" || !isEdit))
            {
                if (lichTrinh.SoGheTrong <= 0)
                {
                    ModelState.AddModelError("", "Lịch trình này đã hết chỗ ngồi.");
                }
            }
        }

        private async Task UpdateSoGheTrong(Ve ve, bool isEdit = false, string? trangThaiCu = null)
        {
            var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);
            if (lichTrinh == null) return;

            if (!isEdit) // Trường hợp Create
            {
                if (ve.TrangThai != "Đã hủy") lichTrinh.SoGheTrong--;
            }
            else // Trường hợp Edit (Chỉ update nếu có sự thay đổi về trạng thái chiếm chỗ)
            {
                if (trangThaiCu == "Đã hủy" && ve.TrangThai != "Đã hủy")
                    lichTrinh.SoGheTrong--;
                else if (trangThaiCu != "Đã hủy" && ve.TrangThai == "Đã hủy")
                    lichTrinh.SoGheTrong++;
            }
        }

        #endregion

        public async Task<IActionResult> Index()
        {
            var listVe = await _context.Ves
                .Include(v => v.Ghe)
                .Include(v => v.LichTrinh!).ThenInclude(lt => lt.Tau)
                .Include(v => v.LichTrinh!).ThenInclude(lt => lt.TuyenDuong)
                .Include(v => v.HoaDon!).ThenInclude(hd => hd.KhachHang)
                .Select(v => new VeViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe!.TenGhe,
                    LoaiGhe = v.Ghe!.LoaiGhe,
                    TenTau = v.LichTrinh!.Tau!.TenTau,
                    ThongTinChuyen = $"{v.LichTrinh!.TuyenDuong!.TenTuyen} ({v.LichTrinh!.NgayGioKhoiHanh:HH:mm dd/MM})",
                    TenKhachHang = v.HoaDon!.KhachHang!.HoTen,
                    MaHoaDon = v.MaHoaDon,
                    GiaVe = v.GiaVe,
                    TrangThai = v.TrangThai
                })
                .OrderByDescending(v => v.MaVe)
                .ToListAsync();

            return View(listVe);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ve = await _context.Ves
                .Include(v => v.Ghe)
                .Include(v => v.LichTrinh).ThenInclude(lt => lt!.Tau)
                .Include(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(v => v.HoaDon).ThenInclude(hd => hd!.KhachHang)
                .FirstOrDefaultAsync(m => m.MaVe == id);

            if (ve == null) return NotFound();

            // Mapping sang ViewModel
            var viewModel = new VeViewModel
            {
                MaVe = ve.MaVe,
                TenGhe = ve.Ghe?.TenGhe,
                LoaiGhe = ve.Ghe?.LoaiGhe,
                TenTau = ve.LichTrinh?.Tau?.TenTau,
                ThongTinChuyen = $"{ve.LichTrinh?.TuyenDuong?.TenTuyen} ({ve.LichTrinh?.NgayGioKhoiHanh:HH:mm dd/MM})",
                TenKhachHang = ve.HoaDon?.KhachHang?.HoTen,
                MaHoaDon = ve.MaHoaDon,
                GiaVe = ve.GiaVe,
                TrangThai = ve.TrangThai
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Ve { TrangThai = "Đã thanh toán" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ve ve)
        {
            ModelState.Remove("HoaDon"); ModelState.Remove("Ghe"); ModelState.Remove("LichTrinh");

            await ValidateVeBusiness(ve, isEdit: false);

            if (ModelState.IsValid)
            {
                await UpdateSoGheTrong(ve, isEdit: false);
                _context.Add(ve);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo vé thành công!";
                return RedirectToAction(nameof(Index));
            }
            LoadDropdownData(ve);
            return View(ve);
        }

        // GET: Admin/Ves/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ve = await _context.Ves.FindAsync(id);
            if (ve == null) return NotFound();

            LoadDropdownData(ve);
            return View(ve);
        }

        // POST: Admin/Ves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ve ve)
        {
            if (id != ve.MaVe) return NotFound();

            // Lấy thông tin vé cũ để kiểm tra thay đổi trạng thái
            var veCu = await _context.Ves.AsNoTracking().FirstOrDefaultAsync(v => v.MaVe == id);
            if (veCu == null) return NotFound();

            ModelState.Remove("HoaDon");
            ModelState.Remove("Ghe");
            ModelState.Remove("LichTrinh");

            // Sử dụng logic Business Rules Huy đã viết
            await ValidateVeBusiness(ve, isEdit: true, trangThaiCu: veCu.TrangThai);

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdateSoGheTrong(ve, isEdit: true, trangThaiCu: veCu.TrangThai);
                    _context.Update(ve);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật vé thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ves.Any(e => e.MaVe == ve.MaVe)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadDropdownData(ve);
            return View(ve);
        }


        // --- AJAX DELETE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ve = await _context.Ves.FindAsync(id);
            if (ve == null) return Json(new { success = false, message = "Không tìm thấy vé." });

            try
            {
                // Nếu xóa vé đang hoạt động thì trả lại ghế cho tàu
                if (ve.TrangThai != "Đã hủy")
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);
                    if (lichTrinh != null) lichTrinh.SoGheTrong++;
                }

                _context.Ves.Remove(ve);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa vé và hoàn trả chỗ ngồi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGhesByLichTrinh(int maLichTrinh, int? maVeHienTai = null)
        {
            var lt = await _context.LichTrinhs.FindAsync(maLichTrinh);
            if (lt == null) return Json(new List<object>());

            var gheDaDatIds = await _context.Ves
                .Where(v => v.MaLichTrinh == maLichTrinh && v.TrangThai != "Đã hủy" && v.MaVe != maVeHienTai)
                .Select(v => v.MaGhe).ToListAsync();

            var ghes = await _context.Ghes
                .Where(g => g.MaTau == lt.MaTau && !gheDaDatIds.Contains(g.MaGhe))
                .Select(g => new { value = g.MaGhe, text = $"{g.TenGhe} ({g.LoaiGhe})" })
                .ToListAsync();

            return Json(ghes);
        }

        #region Helpers
        private void LoadDropdownData(Ve? ve = null)
        {
            var lichTrinhs = _context.LichTrinhs.Include(l => l.Tau).Include(l => l.TuyenDuong)
                .Select(l => new
                {
                    l.MaLichTrinh,
                    Display = $"{l.Tau!.TenTau} - {l.TuyenDuong!.TenTuyen} ({l.NgayGioKhoiHanh:dd/MM HH:mm})"
                }).ToList();
            ViewData["MaLichTrinh"] = new SelectList(lichTrinhs, "MaLichTrinh", "Display", ve?.MaLichTrinh);

            var hoaDons = _context.HoaDons.Include(h => h.KhachHang)
                .Select(h => new { h.MaHoaDon, Display = $"HĐ{h.MaHoaDon} - {h.KhachHang!.HoTen}" }).ToList();
            ViewData["MaHoaDon"] = new SelectList(hoaDons, "MaHoaDon", "Display", ve?.MaHoaDon);

            if (ve != null && ve.MaLichTrinh > 0)
            {
                var lt = _context.LichTrinhs.Find(ve.MaLichTrinh);
                if (lt != null)
                {
                    var ghes = _context.Ghes.Where(g => g.MaTau == lt.MaTau)
                        .Select(g => new { g.MaGhe, Display = $"{g.TenGhe} ({g.LoaiGhe})" }).ToList();
                    ViewData["MaGhe"] = new SelectList(ghes, "MaGhe", "Display", ve.MaGhe);
                }
            }
        }
        #endregion
    }
}