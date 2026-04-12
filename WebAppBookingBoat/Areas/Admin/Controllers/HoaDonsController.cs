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

        public HoaDonsController(ApplicationDbContext context, UserManager<AppUser> userManager) { _context = context; _userManager = userManager; }

        // 1. Hiển thị danh sách hóa đơn
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

        // 2. Chi tiết hóa đơn và các vé đi kèm
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Lấy dữ liệu từ DB (Include đầy đủ các bảng liên quan)
            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.KhuyenMai)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null) return NotFound();

            // Ánh xạ dữ liệu sang ViewModel
            var viewModel = new HoaDonViewModel
            {
                MaHoaDon = hoaDon.MaHoaDon,
                TenKhachHang = hoaDon.KhachHang?.HoTen ?? "Khách vãng lai",
                SoDienThoaiKH = hoaDon.KhachHang?.Sdt ?? "N/A",
                TenNhanVien = hoaDon.NhanVien?.HoTen ?? "Hệ thống",
                NgayLap = hoaDon.NgayLap,
                SoLuongVe = hoaDon.SoLuongVe,
                TamTinh = hoaDon.TamTinh,
                SoTienGiam = hoaDon.SoTienGiam,
                TongTien = hoaDon.TongTien,
                PhuongThucTT = hoaDon.PhuongThucTT,
                TrangThai = hoaDon.TrangThai,
                GhiChu = hoaDon.GhiChu,
                // Ánh xạ danh sách vé con
                DanhSachVe = hoaDon.Ves.Select(v => new VeChiTietViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe?.TenGhe ?? "N/A",
                    LoaiGhe = v.Ghe?.LoaiGhe ?? "Thường",
                    GiaVe = v.GiaVe > 0 ? v.GiaVe : (v.Ghe?.LoaiGhe == "VIP" ? (v.LichTrinh?.GiaVeCoBan * 1.2m ?? 0) : (v.LichTrinh?.GiaVeCoBan ?? 0)),
                    TrangThai = hoaDon.TrangThai switch
                    {
                        "Đã thanh toán" => "Hợp lệ",
                        "Chưa thanh toán" => "Đang chờ",
                        "Đã hủy" => "Đã hủy",
                        _ => v.TrangThai // Mặc định nếu có trạng thái khác
                    }
                }).ToList()
            };

            return View(viewModel);
        }

        // 3. GET: Create
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
            // 1. Kiểm tra logic chọn ghế
            if (vm.SelectedVeIds == null || !vm.SelectedVeIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một ghế trên sơ đồ.");
            }

            // 2. Kiểm tra logic khách hàng (Custom Validation)
            if (vm.IsVangLai)
            {
                if (string.IsNullOrWhiteSpace(vm.TenKhachVangLai))
                    ModelState.AddModelError("TenKhachVangLai", "Vui lòng nhập tên khách vãng lai.");
            }
            else
            {
                if (vm.MaKH == null || vm.MaKH == 0)
                    ModelState.AddModelError("MaKH", "Vui lòng chọn một khách hàng từ hệ thống.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    int? maKH_Final = vm.MaKH;

                    var userId = _userManager.GetUserId(User);
                    var nhanVienThucTe = _context.NhanViens
                        .FirstOrDefault(nv => nv.MaTK!.Trim().Equals(userId));


                    if (nhanVienThucTe == null)
                    {
                        ModelState.AddModelError("", "Lỗi: Tài khoản này chưa được gán vào bất kỳ nhân viên nào trong danh sách Nhân Viên!");
                        await PopulateListsAsync(vm);
                        return View(vm);
                    }

                    // 3. Xử lý lưu khách hàng mới nếu là vãng lai
                    if (vm.IsVangLai)
                    {
                        var khMoi = new KhachHang
                        {
                            HoTen = vm.TenKhachVangLai!,
                            Sdt = vm.SdtKhachVangLai!,
                            Email = vm.EmailKhachVangLai!,
                        };
                        _context.KhachHangs.Add(khMoi);
                        await _context.SaveChangesAsync();
                        maKH_Final = khMoi.MaKH;
                    }

                    // 4. Khởi tạo và lưu Hóa đơn
                    var hoaDon = new HoaDon
                    {
                        MaKH = maKH_Final ?? 0,
                        MaNV = nhanVienThucTe!.MaNV,
                        MaKM = vm.MaKM,
                        NgayLap = DateTime.Now,
                        PhuongThucTT = vm.PhuongThucTT,
                        TrangThai = vm.TrangThai,
                        GhiChu = vm.GhiChu,
                        TamTinh = vm.TamTinh,
                        SoTienGiam = vm.SoTienGiam,
                        TongTien = vm.TongTien,
                        SoLuongVe = vm.SelectedVeIds!.Count,
                        NgayThanhToan = vm.TrangThai == "Đã thanh toán" ? DateTime.Now : null
                    };

                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();

                    // 5. Tạo các bản ghi Vé
                    foreach (var maGhe in vm.SelectedVeIds)
                    {
                        // Lấy thông tin ghế để xác định giá (VIP hoặc Thường)
                        var ghe = await _context.Ghes.FindAsync(maGhe);
                        var lichTrinh = await _context.LichTrinhs.FindAsync(vm.MaLichTrinh);

                        // Tính toán giá vé thực tế dựa trên loại ghế
                        decimal giaThucTe = (ghe?.LoaiGhe == "VIP")
                                            ? (lichTrinh?.GiaVeCoBan * 1.2m ?? 0)
                                            : (lichTrinh?.GiaVeCoBan ?? 0);

                        var newVe = new Ve
                        {
                            MaHoaDon = hoaDon.MaHoaDon,
                            MaLichTrinh = vm.MaLichTrinh,
                            MaGhe = maGhe,
                            GiaVe = giaThucTe, // Lưu giá vé vào DB để Details không bị lỗi hiển thị
                            TrangThai = hoaDon.TrangThai switch
                            {
                                "Đã thanh toán" => "Hợp lệ",
                                "Chưa thanh toán" => "Đang chờ",
                                "Đã hủy" => "Đã hủy",
                                _ => hoaDon.TrangThai // Dùng biến hoaDon (viết thường)
                            }
                        };
                        _context.Ves.Add(newVe);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            // Nếu có lỗi, load lại dữ liệu cho các Dropdown
            await PopulateListsAsync(vm);
            return View(vm);
        }

        // 5. Xóa hóa đơn (Xóa hóa đơn thì phải giải phóng vé)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ves).FirstOrDefaultAsync(h => h.MaHoaDon == id);
            if (hoaDon != null)
            {
                // Giải phóng vé trước khi xóa hóa đơn
                foreach (var ve in hoaDon.Ves)
                {
                    ve.MaHoaDon = null;
                    ve.TrangThai = "Còn trống";
                }
                _context.HoaDons.Remove(hoaDon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int id, string status, string method)
        {
            // 1. Load hóa đơn kèm danh sách vé
            var hoaDon = await _context.HoaDons
                .Include(h => h.Ves)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null)
                return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

            // 2. Cập nhật thông tin thanh toán cho Hóa đơn
            hoaDon.TrangThai = status;
            hoaDon.PhuongThucTT = method;

            if (status == "Đã thanh toán")
            {
                // Cập nhật ngày thanh toán là thời điểm hiện tại
                hoaDon.NgayThanhToan = DateTime.Now;

                // 3. Cập nhật trạng thái tất cả các vé đi kèm sang "Hợp lệ"
                if (hoaDon.Ves != null)
                {
                    foreach (var ve in hoaDon.Ves)
                    {
                        ve.TrangThai = "Hợp lệ";
                    }
                }
            }
            else if (status == "Đã hủy")
            {
                hoaDon.NgayThanhToan = null; // Nếu hủy thì xóa ngày thanh toán (nếu có)
                if (hoaDon.Ves != null)
                {
                    foreach (var ve in hoaDon.Ves)
                    {
                        ve.TrangThai = "Đã hủy";
                    }
                }
            }

            try
            {
                _context.Update(hoaDon);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã cập nhật trạng thái thanh toán thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật dữ liệu: " + ex.Message });
            }
        }

        // --- HÀM TRỢ GIÚP (HELPER) ---

        private async Task PopulateListsAsync(HoaDonCreateViewModel vm)
        {
            vm.DanhSachKhachHang = new SelectList(await _context.KhachHangs.ToListAsync(), "MaKH", "HoTen", vm.MaKH);
            vm.DanhSachKhuyenMai = new SelectList(await _context.KhuyenMais.Where(k => k.TrangThai).ToListAsync(), "MaKM", "TenChuongTrinh", vm.MaKM);

            var dsLT = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .OrderByDescending(lt => lt.NgayGioKhoiHanh)
                .Select(lt => new
                {
                    MaLT = lt.MaLichTrinh,
                    // Hiển thị dạng: [Sài Gòn - Cần Giờ] - 15/04 08:30
                    Text = $"[{lt.TuyenDuong.TenTuyen}] - {lt.NgayGioKhoiHanh:dd/MM HH:mm}"
                }).ToListAsync();

            vm.DanhSachLichTrinh = new SelectList(dsLT, "MaLT", "Text", vm.MaLichTrinh);
        }

        [HttpGet]
        public async Task<JsonResult> GetGhesByLichTrinh(int maLT)
        {
            var lichTrinh = await _context.LichTrinhs
                .Include(lt => lt.Tau)
                .ThenInclude(t => t.Ghes)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == maLT);

            if (lichTrinh == null) return Json(new List<object>());

            // Lấy danh sách MaGhe đã được tạo vé cho lịch trình này
            var gheDaCoVe = await _context.Ves
                .Where(v => v.MaLichTrinh == maLT)
                .Select(v => v.MaGhe)
                .ToListAsync();

            var giaCoBan = lichTrinh.GiaVeCoBan;

            var soDoGhe = lichTrinh.Tau.Ghes.Select(ghe => new
            {
                maGhe = ghe.MaGhe,
                tenGhe = ghe.TenGhe,
                loaiGhe = ghe.LoaiGhe, // VIP hoặc Thường
                                       // Nếu là VIP thì tính giá khác, Thường tính giá khác
                giaThucTe = ghe.LoaiGhe == "VIP" ? (giaCoBan * 1.2m) : giaCoBan,
                // CHỈ CÓ ĐIỀU KIỆN NÀY: Nếu chưa có bản ghi Vé thì là trống
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
    }
}