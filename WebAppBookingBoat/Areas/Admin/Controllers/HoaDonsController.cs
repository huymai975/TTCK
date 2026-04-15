using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HoaDonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HoaDonsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region PRIVATE LOGIC & HELPERS

        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var userId = _userManager.GetUserId(User);
            var log = new Log
            {
                MaTK = userId,
                HanhDong = hanhDong,
                BangTacDong = "HoaDons",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        private async Task PopulateListsAsync(HoaDonCreateViewModel vm)
        {
            vm.DanhSachKhachHang = new SelectList(await _context.KhachHangs.OrderBy(k => k.HoTen).ToListAsync(), "MaKH", "HoTen", vm.MaKH);
            vm.DanhSachKhuyenMai = new SelectList(await _context.KhuyenMais.Where(k => k.TrangThai == "Đang diễn ra").ToListAsync(), "MaKM", "TenChuongTrinh", vm.MaKM);

            var dsLT = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .Where(lt => lt.TrangThai == "Sắp khởi hành" && lt.NgayGioKhoiHanh > DateTime.Now)
                .OrderBy(lt => lt.NgayGioKhoiHanh)
                .Select(lt => new
                {
                    MaLT = lt.MaLichTrinh,
                    Text = $"[{lt.TuyenDuong.TenTuyen}] - {lt.NgayGioKhoiHanh:dd/MM HH:mm} (Trống: {lt.SoGheTrong})"
                }).ToListAsync();

            vm.DanhSachLichTrinh = new SelectList(dsLT, "MaLT", "Text", vm.MaLichTrinh);
        }

        #endregion

        #region ACTION METHODS

        public async Task<IActionResult> Index()
        {
            var hoadons = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.KhuyenMai)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            ViewBag.DanhSachTuyen = hoadons
        .SelectMany(h => h.Ves)
        .Select(v => v.LichTrinh?.TuyenDuong?.TenTuyen)
        .Where(t => t != null)
        .Distinct()
        .ToList();
            return View(hoadons);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.KhuyenMai)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.Tau)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null) return NotFound();

            var veDauTien = hoaDon.Ves?.FirstOrDefault();
            var lichTrinh = veDauTien?.LichTrinh;

            var viewModel = new HoaDonViewModel
            {
                MaHoaDon = hoaDon.MaHoaDon,
                TenKhachHang = hoaDon.KhachHang?.HoTen ?? "Khách vãng lai",
                SoDienThoaiKH = hoaDon.KhachHang?.Sdt ?? "N/A",
                TenNhanVien = hoaDon.NhanVien?.HoTen ?? "Hệ thống",
                NgayLap = hoaDon.NgayLap,
                TenChuyen = lichTrinh != null ? $"Khởi hành {lichTrinh.NgayGioKhoiHanh:HH:mm}" : "N/A",
                TuyenDuong = lichTrinh?.TuyenDuong?.TenTuyen ?? "N/A",
                NgayKhoiHanh = lichTrinh?.NgayGioKhoiHanh ?? DateTime.MinValue,
                TenTau = lichTrinh?.Tau?.TenTau ?? "N/A",
                SoLuongVe = hoaDon.SoLuongVe,
                TamTinh = hoaDon.TamTinh,
                SoTienGiam = hoaDon.SoTienGiam,
                TongTien = hoaDon.TongTien,
                PhuongThucTT = hoaDon.PhuongThucTT,
                TrangThai = hoaDon.TrangThai,
                GhiChu = hoaDon.GhiChu,
                DanhSachVe = hoaDon.Ves?.Select(v => new VeChiTietViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe?.TenGhe ?? "N/A",
                    LoaiGhe = v.Ghe?.LoaiGhe ?? "Thường",
                    GiaVe = v.GiaVe,
                    TrangThai = v.TrangThai
                }).ToList() ?? new List<VeChiTietViewModel>()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new HoaDonCreateViewModel();
            await PopulateListsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoaDonCreateViewModel vm)
        {
            if (vm.SelectedVeIds == null || !vm.SelectedVeIds.Any())
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một chỗ ngồi.");

            var checkLichTrinh = await _context.LichTrinhs.FindAsync(vm.MaLichTrinh);
            if (checkLichTrinh == null)
                ModelState.AddModelError("", "Lịch trình không hợp lệ.");
            else if (checkLichTrinh.TrangThai != "Sắp khởi hành")
                ModelState.AddModelError("", "Chỉ có thể đặt vé cho lịch trình 'Sắp khởi hành'.");

            if (vm.IsVangLai)
            {
                if (string.IsNullOrWhiteSpace(vm.TenKhachVangLai))
                    ModelState.AddModelError("TenKhachVangLai", "Tên khách vãng lai không được để trống.");
            }
            else if (vm.MaKH == null || vm.MaKH == 0)
            {
                ModelState.AddModelError("MaKH", "Vui lòng chọn khách hàng từ danh sách.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    int? maKH_Final = vm.MaKH;
                    var userId = _userManager.GetUserId(User);
                    var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.MaTK == userId);

                    if (nhanVien == null)
                    {
                        ModelState.AddModelError("", "Lỗi: Tài khoản quản trị viên chưa được liên kết với hồ sơ Nhân viên.");
                        await PopulateListsAsync(vm);
                        return View(vm);
                    }

                    if (vm.IsVangLai)
                    {
                        var khMoi = new KhachHang { HoTen = vm.TenKhachVangLai!, Sdt = vm.SdtKhachVangLai!, Email = vm.EmailKhachVangLai! };
                        _context.KhachHangs.Add(khMoi);
                        await _context.SaveChangesAsync();
                        maKH_Final = khMoi.MaKH;
                    }

                    var hoaDon = new HoaDon
                    {
                        MaKH = maKH_Final ?? 0,
                        MaNV = nhanVien.MaNV,
                        MaKM = vm.MaKM,
                        NgayLap = DateTime.Now,
                        PhuongThucTT = vm.PhuongThucTT,
                        TrangThai = vm.TrangThai,
                        TamTinh = vm.TamTinh,
                        SoTienGiam = vm.SoTienGiam,
                        TongTien = vm.TongTien,
                        SoLuongVe = vm.SelectedVeIds!.Count,
                        NgayThanhToan = vm.TrangThai == "Đã thanh toán" ? DateTime.Now : null
                    };

                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();

                    foreach (var maGhe in vm.SelectedVeIds)
                    {
                        var ghe = await _context.Ghes.FindAsync(maGhe);
                        decimal giaThucTe = (ghe?.LoaiGhe == "VIP") ? (checkLichTrinh!.GiaVeCoBan * 1.2m) : (checkLichTrinh!.GiaVeCoBan);

                        _context.Ves.Add(new Ve
                        {
                            MaHoaDon = hoaDon.MaHoaDon,
                            MaLichTrinh = vm.MaLichTrinh,
                            MaGhe = maGhe,
                            GiaVe = giaThucTe,
                            TrangThai = hoaDon.TrangThai == "Đã thanh toán" ? "Hợp lệ" : "Đang chờ"
                        });
                    }

                    checkLichTrinh!.SoGheTrong = Math.Max(0, checkLichTrinh.SoGheTrong - vm.SelectedVeIds.Count);
                    _context.LichTrinhs.Update(checkLichTrinh);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await GhiLogHeThong("Lập hóa đơn", $"Tạo HD #{hoaDon.MaHoaDon} - Tổng tiền: {hoaDon.TongTien:N0}đ");
                    TempData["SuccessMessage"] = $"Lập hóa đơn #{hoaDon.MaHoaDon} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await GhiLogHeThong("Lỗi nghiệp vụ", ex.Message, "Error");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            await PopulateListsAsync(vm);
            return View(vm);
        }

        public async Task<IActionResult> XuatHoaDon(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.Tau)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null) return NotFound();

            var veDau = hoaDon.Ves?.FirstOrDefault();
            var lt = veDau?.LichTrinh;

            var viewModel = new HoaDonViewModel
            {
                MaHoaDon = hoaDon.MaHoaDon,
                TenKhachHang = hoaDon.KhachHang?.HoTen ?? "Khách lẻ",
                SoDienThoaiKH = hoaDon.KhachHang?.Sdt ?? "",
                TenNhanVien = hoaDon.NhanVien?.HoTen ?? "Hệ thống",
                NgayLap = hoaDon.NgayLap,
                TenTau = lt?.Tau?.TenTau ?? "N/A",
                TuyenDuong = lt?.TuyenDuong != null ? $"{lt.TuyenDuong.DiemDi} - {lt.TuyenDuong.DiemDen}" : "N/A",
                NgayKhoiHanh = lt?.NgayGioKhoiHanh ?? DateTime.Now,
                SoLuongVe = hoaDon.SoLuongVe,
                TamTinh = hoaDon.TamTinh,
                SoTienGiam = hoaDon.SoTienGiam,
                TongTien = hoaDon.TongTien,
                PhuongThucTT = hoaDon.PhuongThucTT,
                TrangThai = hoaDon.TrangThai,
                DanhSachVe = hoaDon.Ves?.Select(v => new VeChiTietViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe?.TenGhe ?? "N/A",
                    LoaiGhe = v.Ghe?.LoaiGhe ?? "Thường",
                    GiaVe = v.GiaVe
                }).ToList() ?? new List<VeChiTietViewModel>()
            };

            return new ViewAsPdf("XuatHoaDonPDF", viewModel)
            {
                FileName = $"HoaDon_{id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                CustomSwitches = "--print-media-type"
            };
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int id, string status, string method)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

            try
            {
                string oldStatus = hoaDon.TrangThai;
                var lichTrinh = hoaDon.Ves.FirstOrDefault()?.LichTrinh;

                if (status == "Đã hủy")
                {
                    if (lichTrinh != null)
                    {
                        if (lichTrinh.TrangThai == "Hoàn thành")
                            return Json(new { success = false, message = "Hành trình đã kết thúc, không thể hủy." });


                        if (oldStatus != "Đã hủy")
                        {
                            lichTrinh.SoGheTrong += hoaDon.Ves.Count;
                            _context.LichTrinhs.Update(lichTrinh);
                        }
                    }
                    foreach (var v in hoaDon.Ves) v.TrangThai = "Đã hủy";
                    hoaDon.NgayThanhToan = null;
                }
                else if (status == "Đã thanh toán")
                {
                    hoaDon.NgayThanhToan = DateTime.Now;
                    foreach (var v in hoaDon.Ves) v.TrangThai = "Hợp lệ";
                }

                hoaDon.TrangThai = status;
                hoaDon.PhuongThucTT = method;

                await _context.SaveChangesAsync();
                await GhiLogHeThong("Cập nhật thanh toán", $"HD #{id}: {oldStatus} -> {status}");

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ves).ThenInclude(v => v.LichTrinh).FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon != null)
            {
                var lichTrinh = hoaDon.Ves.FirstOrDefault()?.LichTrinh;
                if (lichTrinh != null && (lichTrinh.TrangThai == "Hoàn thành" || (lichTrinh.NgayGioKhoiHanh - DateTime.Now).TotalHours < 1))
                {
                    TempData["ErrorMessage"] = "Không thể xóa hóa đơn do vi phạm quy định thời gian hoặc trạng thái chuyến đi.";
                    return RedirectToAction(nameof(Index));
                }

                if (lichTrinh != null && hoaDon.TrangThai != "Đã hủy")
                {
                    lichTrinh.SoGheTrong += hoaDon.Ves.Count;
                    _context.LichTrinhs.Update(lichTrinh);
                }

                _context.HoaDons.Remove(hoaDon);
                await _context.SaveChangesAsync();
                await GhiLogHeThong("Xóa hóa đơn", $"Xóa vĩnh viễn HD #{id}", "Warning");
                TempData["SuccessMessage"] = "Đã xóa hóa đơn khỏi hệ thống.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region AJAX API

        [HttpGet]
        public async Task<JsonResult> GetGhesByLichTrinh(int maLT)
        {
            var lichTrinh = await _context.LichTrinhs
                .Include(lt => lt.Tau).ThenInclude(t => t.Ghes)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == maLT);

            if (lichTrinh == null) return Json(new List<object>());

            var gheDaBan = await _context.Ves
                .Where(v => v.MaLichTrinh == maLT && v.TrangThai != "Đã hủy")
                .Select(v => v.MaGhe)
                .ToListAsync();

            var result = lichTrinh.Tau.Ghes.Select(g => new
            {
                maGhe = g.MaGhe,
                tenGhe = g.TenGhe,
                loaiGhe = g.LoaiGhe,
                giaThucTe = g.LoaiGhe == "VIP" ? (lichTrinh.GiaVeCoBan * 1.2m) : lichTrinh.GiaVeCoBan,
                isAvailable = !gheDaBan.Contains(g.MaGhe)
            }).OrderBy(g => g.tenGhe).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetKhuyenMaiInfo(string maKM)
        {
            var km = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.MaKM == maKM && k.TrangThai == "Đang diễn ra");
            return Json(km == null ? null : new { phanTram = km.PhanTramGiam, toiDa = km.SoTienToiDaGiam });
        }

        #endregion
    }
}