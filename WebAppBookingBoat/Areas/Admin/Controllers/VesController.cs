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

        // GET: Admin/Ves
        public async Task<IActionResult> Index()
        {
            var listVe = await _context.Ves
                .Include(v => v.Ghe)
                .Include(v => v.LichTrinh).ThenInclude(lt => lt!.Tau) // Fix CS8602
                .Include(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(v => v.HoaDon).ThenInclude(hd => hd!.KhachHang)
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

        // GET: Admin/Ves/Details/5
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

            // Chuyển đổi sang ViewModel để hiển thị đẹp hơn trên View Details
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

        // GET: Admin/Ves/Create
        public IActionResult Create()
        {
            LoadDropdownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaVe,MaHoaDon,MaLichTrinh,MaGhe,GiaVe,TrangThai")] Ve ve)
        {
            // Xóa validation navigation properties
            ModelState.Remove("HoaDon");
            ModelState.Remove("Ghe");
            ModelState.Remove("LichTrinh");

            if (ModelState.IsValid)
            {
                var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);

                // Kiểm tra ghế trống
                if (lichTrinh == null || lichTrinh.SoGheTrong <= 0)
                {
                    ModelState.AddModelError("", "Lịch trình này đã hết chỗ trống.");
                }
                // Kiểm tra trùng ghế
                else if (await _context.Ves.AnyAsync(v => v.MaLichTrinh == ve.MaLichTrinh && v.MaGhe == ve.MaGhe && v.TrangThai != "Đã hủy"))
                {
                    ModelState.AddModelError("MaGhe", "Ghế này đã có người ngồi.");
                }
                else
                {
                    // Cập nhật số ghế trống
                    if (ve.TrangThai != "Đã hủy") lichTrinh.SoGheTrong--;

                    _context.Add(ve);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tạo vé thành công!";
                    return RedirectToAction(nameof(Index));
                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaVe,MaHoaDon,MaLichTrinh,MaGhe,GiaVe,TrangThai")] Ve ve)
        {
            if (id != ve.MaVe) return NotFound();

            // Xóa validation navigation properties
            ModelState.Remove("HoaDon");
            ModelState.Remove("Ghe");
            ModelState.Remove("LichTrinh");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ để so sánh
                    var veCu = await _context.Ves.AsNoTracking().FirstOrDefaultAsync(v => v.MaVe == id);
                    if (veCu == null) return NotFound();

                    var ltHienTai = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);

                    // Kiểm tra trùng ghế (trừ chính nó)
                    var isGheTrung = await _context.Ves.AnyAsync(v =>
                        v.MaLichTrinh == ve.MaLichTrinh && v.MaGhe == ve.MaGhe &&
                        v.MaVe != ve.MaVe && v.TrangThai != "Đã hủy");

                    if (isGheTrung)
                    {
                        ModelState.AddModelError("MaGhe", "Ghế này đã được đặt.");
                    }
                    else
                    {
                        // Xử lý đổi lịch trình
                        if (veCu.MaLichTrinh != ve.MaLichTrinh)
                        {
                            var ltCu = await _context.LichTrinhs.FindAsync(veCu.MaLichTrinh);
                            if (ve.TrangThai != "Đã hủy" && (ltHienTai == null || ltHienTai.SoGheTrong <= 0))
                            {
                                ModelState.AddModelError("MaLichTrinh", "Chuyến tàu mới đã hết chỗ.");
                                LoadDropdownData(ve);
                                return View(ve);
                            }
                            if (veCu.TrangThai != "Đã hủy" && ltCu != null) ltCu.SoGheTrong++;
                            if (ve.TrangThai != "Đã hủy" && ltHienTai != null) ltHienTai.SoGheTrong--;
                        }
                        // Xử lý đổi trạng thái vé
                        else if (veCu.TrangThai != ve.TrangThai)
                        {
                            if (ve.TrangThai == "Đã hủy") ltHienTai!.SoGheTrong++;
                            else if (veCu.TrangThai == "Đã hủy")
                            {
                                if (ltHienTai!.SoGheTrong <= 0)
                                {
                                    ModelState.AddModelError("TrangThai", "Không thể khôi phục vì tàu đã đầy.");
                                    LoadDropdownData(ve);
                                    return View(ve);
                                }
                                ltHienTai.SoGheTrong--;
                            }
                        }

                        _context.Update(ve);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            LoadDropdownData(ve);
            return View(ve);
        }


        // GET: Admin/Ves/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ve = await _context.Ves
                .Include(v => v.HoaDon).ThenInclude(h => h!.KhachHang)
                .FirstOrDefaultAsync(m => m.MaVe == id);

            if (ve == null) return NotFound();

            return View(ve);
        }

        // POST: Admin/Ves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ve = await _context.Ves.FindAsync(id);
            if (ve != null)
            {
                // QUAN TRỌNG: Cập nhật lại số ghế trống khi xóa vé
                if (ve.TrangThai != "Đã hủy")
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);
                    if (lichTrinh != null)
                    {
                        lichTrinh.SoGheTrong++;
                        _context.Update(lichTrinh); // Đảm bảo trạng thái lịch trình được cập nhật
                    }
                }

                _context.Ves.Remove(ve);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa vé thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API dùng cho Ajax lọc ghế
        [HttpGet]
        public async Task<JsonResult> GetGhesByLichTrinh(int maLichTrinh, int? maVeHienTai = null)
        {
            var lichTrinh = await _context.LichTrinhs.FirstOrDefaultAsync(lt => lt.MaLichTrinh == maLichTrinh);
            if (lichTrinh == null) return Json(new List<object>());

            var gheDaDatIds = await _context.Ves
                .Where(v => v.MaLichTrinh == maLichTrinh && v.TrangThai != "Đã hủy" && v.MaVe != maVeHienTai)
                .Select(v => v.MaGhe)
                .ToListAsync();

            var ghes = await _context.Ghes
                .Where(g => g.MaTau == lichTrinh.MaTau && !gheDaDatIds.Contains(g.MaGhe))
                .Select(g => new { value = g.MaGhe, text = $"{g.TenGhe} ({g.LoaiGhe})" })
                .ToListAsync();

            return Json(ghes);
        }

        #region Helpers
        private void LoadDropdownData(Ve? ve = null)
        {
            var lichTrinhs = _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .Select(l => new
                {
                    l.MaLichTrinh,
                    Display = $"{(l.Tau != null ? l.Tau.TenTau : "N/A")} - {(l.TuyenDuong != null ? l.TuyenDuong.TenTuyen : "N/A")} (Trống: {l.SoGheTrong})"
                }).ToList();
            ViewData["MaLichTrinh"] = new SelectList(lichTrinhs, "MaLichTrinh", "Display", ve?.MaLichTrinh);

            var hoaDons = _context.HoaDons.Include(h => h.KhachHang).Select(h => new
            {
                h.MaHoaDon,
                Display = $"HĐ{h.MaHoaDon} - {(h.KhachHang != null ? h.KhachHang.HoTen : "N/A")}"
            }).ToList();
            ViewData["MaHoaDon"] = new SelectList(hoaDons, "MaHoaDon", "Display", ve?.MaHoaDon);

            // Xử lý hiển thị lại danh sách ghế khi Load lại trang (Create/Edit)
            if (ve != null && ve.MaLichTrinh > 0)
            {
                var lt = _context.LichTrinhs.FirstOrDefault(l => l.MaLichTrinh == ve.MaLichTrinh);
                if (lt != null)
                {
                    // Lấy tất cả ghế thuộc tàu của lịch trình đó
                    var ghes = _context.Ghes
                        .Where(g => g.MaTau == lt.MaTau)
                        .Select(g => new { g.MaGhe, Display = $"{g.TenGhe} ({g.LoaiGhe})" })
                        .ToList();

                    ViewData["MaGhe"] = new SelectList(ghes, "MaGhe", "Display", ve.MaGhe);
                }
                else
                {
                    ViewData["MaGhe"] = new SelectList(Enumerable.Empty<SelectListItem>());
                }
            }
            else
            {
                ViewData["MaGhe"] = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
        #endregion
    }
}