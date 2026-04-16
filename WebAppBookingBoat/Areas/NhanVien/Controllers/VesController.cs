using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "Staff,Admin")] // Cho phép cả nhân viên và admin truy cập Area này
    public class VesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public VesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        private async Task TinhGiaVe(Ve ve)
        {
            var lichTrinh = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == ve.MaLichTrinh);

            var ghe = await _context.Ghes.FindAsync(ve.MaGhe);

            if (lichTrinh != null && lichTrinh.TuyenDuong != null && ghe != null)
            {
                decimal giaGoc = lichTrinh.GiaVeCoBan;
                if (ghe.LoaiGhe != null && ghe.LoaiGhe.Contains("VIP", StringComparison.OrdinalIgnoreCase))
                {
                    ve.GiaVe = giaGoc * 1.2m;
                }
                else
                {
                    ve.GiaVe = giaGoc;
                }
            }
        }

        private async Task UpdateSoGheTrong(Ve ve, bool isEdit, string trangThaiCu = null!, int? maLichTrinhCu = null)
        {
            var lichTrinhMoi = await _context.LichTrinhs.Include(lt => lt.Tau)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == ve.MaLichTrinh);

            if (!isEdit)
            {
                if (ve.TrangThai != "Đã hủy" && lichTrinhMoi != null)
                {
                    if (lichTrinhMoi.SoGheTrong > 0) lichTrinhMoi.SoGheTrong--;
                }
            }
            else
            {
                if (maLichTrinhCu.HasValue && maLichTrinhCu != ve.MaLichTrinh)
                {
                    if (trangThaiCu != "Đã hủy")
                    {
                        var ltCu = await _context.LichTrinhs.Include(lt => lt.Tau)
                            .FirstOrDefaultAsync(lt => lt.MaLichTrinh == maLichTrinhCu);
                        if (ltCu != null && ltCu.SoGheTrong < ltCu.Tau!.TongSoGhe) ltCu.SoGheTrong++;
                    }
                    if (ve.TrangThai != "Đã hủy" && lichTrinhMoi != null)
                    {
                        if (lichTrinhMoi.SoGheTrong > 0) lichTrinhMoi.SoGheTrong--;
                    }
                }
                else if (trangThaiCu != ve.TrangThai && lichTrinhMoi != null)
                {
                    if (ve.TrangThai == "Đã hủy" && trangThaiCu != "Đã hủy")
                    {
                        if (lichTrinhMoi.SoGheTrong < lichTrinhMoi.Tau!.TongSoGhe) lichTrinhMoi.SoGheTrong++;
                    }
                    else if (ve.TrangThai != "Đã hủy" && trangThaiCu == "Đã hủy")
                    {
                        if (lichTrinhMoi.SoGheTrong > 0) lichTrinhMoi.SoGheTrong--;
                    }
                }
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
                .Select(v => new nvVeViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe!.TenGhe,
                    LoaiGhe = v.Ghe!.LoaiGhe,
                    TenTau = v.LichTrinh!.Tau!.TenTau,
                    ThongTinChuyen = $"{v.LichTrinh!.TuyenDuong!.TenTuyen} ({v.LichTrinh!.NgayGioKhoiHanh:HH:mm dd/MM})",
                    TenKhachHang = v.HoaDon!.KhachHang!.HoTen,
                    MaHoaDon = v.MaHoaDon ?? 0,
                    GiaVe = v.GiaVe,
                    TrangThai = v.TrangThai,
                    TrangThaiHoaDon = v.HoaDon != null ? v.HoaDon.TrangThai : ""
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

            var viewModel = new nvVeViewModel
            {
                MaVe = ve.MaVe,
                TenGhe = ve.Ghe?.TenGhe,
                LoaiGhe = ve.Ghe?.LoaiGhe,
                TenTau = ve.LichTrinh?.Tau?.TenTau,
                ThongTinChuyen = $"{ve.LichTrinh?.TuyenDuong?.TenTuyen} ({ve.LichTrinh?.NgayGioKhoiHanh:HH:mm dd/MM})",
                TenKhachHang = ve.HoaDon?.KhachHang?.HoTen,
                MaHoaDon = ve.MaHoaDon ?? 0,
                GiaVe = ve.GiaVe,
                TrangThai = ve.TrangThai,
                TrangThaiHoaDon = ve.HoaDon?.TrangThai
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Ve { TrangThai = "Hợp lệ" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ve ve)
        {
            ModelState.Remove("HoaDon"); ModelState.Remove("Ghe"); ModelState.Remove("LichTrinh");

            var hoaDon = await _context.HoaDons.FindAsync(ve.MaHoaDon);
            if (hoaDon != null && hoaDon.TrangThai == "Đã thanh toán")
            {
                ModelState.AddModelError("", "Hóa đơn này đã thanh toán, không thể thêm vé mới!");
                LoadDropdownData(ve);
                return View(ve);
            }

            await TinhGiaVe(ve);
            await ValidateVeBusiness(ve, isEdit: false);

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdateSoGheTrong(ve, isEdit: false, trangThaiCu: null!);
                    _context.Add(ve);
                    await _context.SaveChangesAsync();

                    await UpdateHoaDonTongTien(ve.MaHoaDon ?? 0);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Thêm vé", "Vé", $"Nhân viên tạo vé mới MaVe: {ve.MaVe} cho HĐ{ve.MaHoaDon}. Giá: {ve.GiaVe:N0}đ");

                    TempData["SuccessMessage"] = "Tạo vé thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi Thêm vé", "Vé", $"Lỗi: {ex.Message}", "Error");
                    ModelState.AddModelError("", "Lỗi hệ thống khi tạo vé.");
                }
            }
            LoadDropdownData(ve);
            return View(ve);
        }

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
        public async Task<IActionResult> Edit(int id, Ve ve)
        {
            if (id != ve.MaVe) return NotFound();

            var veCu = await _context.Ves.AsNoTracking().FirstOrDefaultAsync(v => v.MaVe == id);
            if (veCu == null) return NotFound();

            var hoaDon = await _context.HoaDons.FindAsync(ve.MaHoaDon);
            if (hoaDon?.TrangThai == "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa vé thuộc hóa đơn đã thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("HoaDon"); ModelState.Remove("Ghe"); ModelState.Remove("LichTrinh");

            await TinhGiaVe(ve);
            await ValidateVeBusiness(ve, isEdit: true, trangThaiCu: veCu.TrangThai);

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdateSoGheTrong(ve, isEdit: true, trangThaiCu: veCu.TrangThai, maLichTrinhCu: veCu.MaLichTrinh);
                    _context.Update(ve);
                    await _context.SaveChangesAsync();

                    await UpdateHoaDonTongTien(ve.MaHoaDon ?? 0);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật vé", "Vé", $"Nhân viên chỉnh sửa vé ID {id}. Trạng thái: {veCu.TrangThai} -> {ve.TrangThai}");

                    TempData["SuccessMessage"] = "Cập nhật vé thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi Cập nhật vé", "Vé", $"ID: {id}. Lỗi: {ex.Message}", "Error");
                    ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu.");
                }
            }
            LoadDropdownData(ve);
            return View(ve);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ve = await _context.Ves.Include(v => v.HoaDon).FirstOrDefaultAsync(v => v.MaVe == id);
            if (ve == null) return Json(new { success = false, message = "Không tìm thấy vé." });

            if (ve.HoaDon?.TrangThai == "Đã thanh toán")
                return Json(new { success = false, message = "Hóa đơn đã thanh toán. Không thể hủy vé!" });

            if (ve.TrangThai == "Đã hủy")
                return Json(new { success = false, message = "Vé này đã hủy từ trước." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string trangThaiCu = ve.TrangThai;
                ve.TrangThai = "Đã hủy";
                _context.Update(ve);

                await UpdateSoGheTrong(ve, isEdit: true, trangThaiCu: trangThaiCu);
                await _context.SaveChangesAsync();

                if (ve.MaHoaDon.HasValue)
                {
                    await UpdateHoaDonTongTien(ve.MaHoaDon.Value);
                    await _context.SaveChangesAsync();
                }

                await GhiLogHeThong("Hủy vé", "Vé", $"Nhân viên hủy vé ID {id} thuộc HĐ{ve.MaHoaDon}", "Warning");

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Đã hủy vé và cập nhật hóa đơn." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await GhiLogHeThong("Lỗi Hủy vé", "Vé", $"ID: {id}. Lỗi: {ex.Message}", "Error");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGhesByLichTrinh(int maLichTrinh)
        {
            var lichTrinh = await _context.LichTrinhs.FindAsync(maLichTrinh);
            if (lichTrinh == null) return Json(new List<object>());

            var gheDaDatIds = await _context.Ves
                .Where(v => v.MaLichTrinh == maLichTrinh && v.TrangThai != "Đã hủy")
                .Select(v => v.MaGhe)
                .ToListAsync();

            var ghesTrong = await _context.Ghes
                .Where(g => g.MaTau == lichTrinh.MaTau && !gheDaDatIds.Contains(g.MaGhe))
                .Select(g => new { value = g.MaGhe, text = $"{g.TenGhe} ({g.LoaiGhe})" })
                .ToListAsync();

            return Json(ghesTrong);
        }

        [HttpGet]
        public async Task<JsonResult> GetGiaCoBanByLichTrinh(int maLichTrinh)
        {
            var lt = await _context.LichTrinhs.Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(l => l.MaLichTrinh == maLichTrinh);
            return Json(new { giaGoc = lt?.GiaVeCoBan ?? 0 });
        }

        #region Helpers
        private void LoadDropdownData(Ve? ve = null)
        {
            var lichTrinhs = _context.LichTrinhs.Include(l => l.Tau).Include(l => l.TuyenDuong)
                .Select(l => new { l.MaLichTrinh, Display = $"{l.Tau!.TenTau} - {l.TuyenDuong!.TenTuyen} ({l.NgayGioKhoiHanh:dd/MM HH:mm})" }).ToList();
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

        private async Task UpdateHoaDonTongTien(int maHoaDon)
        {
            var hoaDon = await _context.HoaDons.FindAsync(maHoaDon);
            if (hoaDon != null)
            {
                var tatCaVe = await _context.Ves.Where(v => v.MaHoaDon == maHoaDon && v.TrangThai != "Đã hủy").ToListAsync();
                hoaDon.SoLuongVe = tatCaVe.Count;
                hoaDon.TamTinh = tatCaVe.Sum(v => v.GiaVe);
                hoaDon.TongTien = Math.Max(0m, hoaDon.TamTinh - hoaDon.SoTienGiam);
                _context.Update(hoaDon);
            }
        }

        [NonAction]
        private async Task GhiLogHeThong(string hanhDong, string bang, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = bang,
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}