using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
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
            vm.DanhSachKhachHang = new SelectList(await _context.KhachHangs.ToListAsync(), "MaKH", "HoTen", vm.MaKH);
            vm.DanhSachKhuyenMai = new SelectList(await _context.KhuyenMais.Where(k => k.TrangThai == "Đang diễn ra").ToListAsync(), "MaKM", "TenKM", vm.MaKM);

            var dsLT = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .OrderByDescending(lt => lt.NgayGioKhoiHanh)
                .Select(lt => new
                {
                    MaLT = lt.MaLichTrinh,
                    Text = $"[{lt.TuyenDuong.TenTuyen}] - {lt.NgayGioKhoiHanh:dd/MM HH:mm}"
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
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();
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

            var veDauTien = hoaDon.Ves.FirstOrDefault();
            var lichTrinh = veDauTien?.LichTrinh;

            var viewModel = new HoaDonViewModel
            {
                MaHoaDon = hoaDon.MaHoaDon,
                TenKhachHang = hoaDon.KhachHang?.HoTen ?? "Khách vãng lai",
                SoDienThoaiKH = hoaDon.KhachHang?.Sdt ?? "N/A",
                TenNhanVien = hoaDon.NhanVien?.HoTen ?? "Hệ thống",
                NgayLap = hoaDon.NgayLap,
                TenChuyen = lichTrinh != null ? $"Chuyến khởi hành {lichTrinh.NgayGioKhoiHanh:HH:mm}" : "N/A",
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
                DanhSachVe = hoaDon.Ves.Select(v => new VeChiTietViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe?.TenGhe ?? "N/A",
                    LoaiGhe = v.Ghe?.LoaiGhe ?? "Thường",
                    GiaVe = v.GiaVe,
                    TrangThai = v.TrangThai
                }).ToList()
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
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một ghế.");

            if (vm.IsVangLai)
            {
                if (string.IsNullOrWhiteSpace(vm.TenKhachVangLai))
                    ModelState.AddModelError("TenKhachVangLai", "Vui lòng nhập tên khách vãng lai.");
            }
            else if (vm.MaKH == null || vm.MaKH == 0)
            {
                ModelState.AddModelError("MaKH", "Vui lòng chọn một khách hàng.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    int? maKH_Final = vm.MaKH;
                    var userId = _userManager.GetUserId(User);
                    var nhanVienThucTe = _context.NhanViens.FirstOrDefault(nv => nv.MaTK!.Trim().Equals(userId));

                    if (nhanVienThucTe == null)
                    {
                        ModelState.AddModelError("", "Tài khoản chưa gán nhân viên!");
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
                        MaNV = nhanVienThucTe.MaNV,
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
                        var lichTrinh = await _context.LichTrinhs.FindAsync(vm.MaLichTrinh);
                        decimal giaThucTe = (ghe?.LoaiGhe == "VIP") ? (lichTrinh?.GiaVeCoBan * 1.2m ?? 0) : (lichTrinh?.GiaVeCoBan ?? 0);

                        var newVe = new Ve
                        {
                            MaHoaDon = hoaDon.MaHoaDon,
                            MaLichTrinh = vm.MaLichTrinh,
                            MaGhe = maGhe,
                            GiaVe = giaThucTe,
                            TrangThai = hoaDon.TrangThai == "Đã thanh toán" ? "Hợp lệ" : "Đang chờ"
                        };
                        _context.Ves.Add(newVe);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await GhiLogHeThong("Lập hóa đơn", $"Lập HD #{hoaDon.MaHoaDon} cho KH ID:{maKH_Final}. Tổng tiền: {hoaDon.TongTien:N0}đ");

                    TempData["SuccessMessage"] = "Lập hóa đơn #" + hoaDon.MaHoaDon + " thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await GhiLogHeThong("Lỗi lập hóa đơn", ex.Message, "Error");
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            await PopulateListsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int id, string status, string method)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ves).FirstOrDefaultAsync(h => h.MaHoaDon == id);
            if (hoaDon == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

            try
            {
                string oldStatus = hoaDon.TrangThai;
                hoaDon.TrangThai = status;
                hoaDon.PhuongThucTT = method;

                if (status == "Đã thanh toán")
                {
                    hoaDon.NgayThanhToan = DateTime.Now;
                    foreach (var ve in hoaDon.Ves) ve.TrangThai = "Hợp lệ";
                }
                else if (status == "Đã hủy")
                {
                    hoaDon.NgayThanhToan = null;
                    _context.Ves.RemoveRange(hoaDon.Ves); // Giải phóng ghế
                }

                await _context.SaveChangesAsync();
                await GhiLogHeThong("Cập nhật thanh toán", $"HD #{id}: {oldStatus} -> {status} ({method})");

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi cập nhật thanh toán", $"HD #{id}: {ex.Message}", "Error");
                return Json(new { success = false, message = "Lỗi khi cập nhật dữ liệu." });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ves).FirstOrDefaultAsync(h => h.MaHoaDon == id);
            if (hoaDon != null)
            {
                foreach (var ve in hoaDon.Ves)
                {
                    ve.MaHoaDon = null;
                    ve.TrangThai = "Còn trống";
                }
                _context.HoaDons.Remove(hoaDon);
                await _context.SaveChangesAsync();
                await GhiLogHeThong("Xóa hóa đơn", $"Xóa HD #{id} và giải phóng các vé liên quan.", "Warning");
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

            var gheDaCoVe = await _context.Ves
                .Where(v => v.MaLichTrinh == maLT && v.TrangThai != "Đã hủy")
                .Select(v => v.MaGhe)
                .ToListAsync();

            var soDoGhe = lichTrinh.Tau.Ghes.Select(ghe => new
            {
                maGhe = ghe.MaGhe,
                tenGhe = ghe.TenGhe,
                loaiGhe = ghe.LoaiGhe,
                giaThucTe = ghe.LoaiGhe == "VIP" ? (lichTrinh.GiaVeCoBan * 1.2m) : lichTrinh.GiaVeCoBan,
                isAvailable = !gheDaCoVe.Contains(ghe.MaGhe)
            }).OrderBy(g => g.tenGhe).ToList();

            return Json(soDoGhe);
        }

        [HttpGet]
        public async Task<JsonResult> GetKhuyenMaiInfo(string maKM)
        {
            var km = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.MaKM == maKM);
            return Json(km == null ? null : new { phanTram = km.PhanTramGiam, toiDa = km.SoTienToiDaGiam });
        }

        #endregion
    }
}