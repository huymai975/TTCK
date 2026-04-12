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
        public HoaDonsController(ApplicationDbContext context) => _context = context;

        // 1. Hiển thị danh sách hóa đơn
        public async Task<IActionResult> Index()
        {
            var hoadons = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.KhuyenMai)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();
            return View(hoadons);
        }

        // 2. Chi tiết hóa đơn và các vé đi kèm
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.KhuyenMai)
                .Include(h => h.Ves).ThenInclude(v => v.Ghe)
                .Include(h => h.Ves).ThenInclude(v => v.LichTrinh)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }

        // 3. GET: Create
        public async Task<IActionResult> Create()
        {
            var vm = new HoaDonCreateViewModel();
            await PopulateListsAsync(vm);
            return View(vm);
        }

        // 4. POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoaDonCreateViewModel vm)
        {
            if (vm.SelectedVeIds == null || !vm.SelectedVeIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một chỗ ngồi.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Tạo đối tượng Hóa đơn
                    var hoaDon = new HoaDon
                    {
                        MaKH = vm.MaKH,
                        MaNV = vm.MaNV, // Tạm thời để 1 hoặc lấy từ User.Identity nếu có
                        MaKM = vm.MaKM,
                        NgayLap = DateTime.Now,
                        PhuongThucTT = vm.PhuongThucTT,
                        TrangThai = vm.TrangThai,
                        GhiChu = vm.GhiChu,
                        TamTinh = vm.TamTinh,
                        SoTienGiam = vm.SoTienGiam,
                        TongTien = vm.TongTien,
                        SoLuongVe = vm.SelectedVeIds?.Count ?? 0,
                        NgayThanhToan = vm.TrangThai == "Đã thanh toán" ? DateTime.Now : null
                    };


                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync(); // Lưu để lấy MaHoaDon

                    // 2. Tạo mới các bản ghi Vé dựa trên danh sách ID Ghế đã chọn
                    // Lưu ý: vm.SelectedVeIds lúc này chính là danh sách MaGhe gửi từ View lên
                    foreach (var maGhe in vm.SelectedVeIds!)
                    {
                        // Kiểm tra xem ghế này đã được ai đặt cho lịch trình này chưa (Bảo mật thêm)
                        var checkVeExist = await _context.Ves.AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh && v.MaGhe == maGhe);
                        if (checkVeExist)
                        {
                            throw new Exception($"Ghế mã số {maGhe} đã có người đặt trước đó.");
                        }

                        var newVe = new Ve
                        {
                            MaHoaDon = hoaDon.MaHoaDon,
                            MaLichTrinh = vm.MaLichTrinh,
                            MaGhe = maGhe,
                            // Trạng thái vé: Nếu trả tiền rồi thì 'Đã sử dụng' (khóa), chưa thì 'Hợp lệ'
                            TrangThai = (hoaDon.TrangThai == "Đã thanh toán") ? "Hợp lệ" : "Đang chờ"
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

        // --- HÀM TRỢ GIÚP (HELPER) ---

        private async Task PopulateListsAsync(HoaDonCreateViewModel vm)
        {
            vm.DanhSachKhachHang = new SelectList(await _context.KhachHangs.ToListAsync(), "MaKH", "HoTen", vm.MaKH);
            vm.DanhSachKhuyenMai = new SelectList(await _context.KhuyenMais.Where(k => k.TrangThai).ToListAsync(), "MaKM", "TenChuongTrinh", vm.MaKM);

            var dsLT = await _context.LichTrinhs
                .Select(lt => new
                {
                    MaLT = lt.MaLichTrinh,
                    Text = $"Chuyến {lt.MaLichTrinh} - {lt.NgayGioKhoiHanh:dd/MM/yyyy HH:mm}"
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