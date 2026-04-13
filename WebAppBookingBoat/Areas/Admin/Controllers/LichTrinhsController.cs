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
    public class LichTrinhsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LichTrinhsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region READ (Index & Details)

        public async Task<IActionResult> Index()
        {
            var bayGio = DateTime.Now;

            // 1. Tự động cập nhật trạng thái
            var lichTrinhsUpdate = await _context.LichTrinhs
                .Where(l => l.TrangThai == "Sắp khởi hành" || l.TrangThai == "Đang vận hành")
                .ToListAsync();

            bool coThayDoi = false;
            foreach (var item in lichTrinhsUpdate)
            {
                var trangThaiGoc = item.TrangThai;
                if (bayGio >= item.NgayGioCapBenDuKien) item.TrangThai = "Hoàn thành";
                else if (bayGio >= item.NgayGioKhoiHanh) item.TrangThai = "Đang vận hành";

                if (trangThaiGoc != item.TrangThai)
                {
                    coThayDoi = true;
                    // Ghi log tự động cập nhật trạng thái (Dùng hệ thống ghi)
                    await GhiLogHeThong("Hệ thống cập nhật trạng thái", "LichTrinhs",
                        $"Lịch trình {item.MaLichTrinh} tự động chuyển: {trangThaiGoc} -> {item.TrangThai}", "Auto");
                }
            }
            if (coThayDoi) await _context.SaveChangesAsync();

            // 2. Lấy dữ liệu và tính toán ghế thực tế
            var list = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .OrderByDescending(l => l.NgayGioKhoiHanh)
                .Select(l => new LichTrinhViewModel
                {
                    MaLichTrinh = l.MaLichTrinh,
                    NgayGioKhoiHanh = l.NgayGioKhoiHanh,
                    NgayGioCapBenDuKien = l.NgayGioCapBenDuKien,
                    GiaVeCoBan = l.GiaVeCoBan,
                    TrangThai = l.TrangThai,
                    TenTuyen = l.TuyenDuong!.TenTuyen,
                    DiemDi = l.TuyenDuong!.DiemDi,
                    DiemDen = l.TuyenDuong!.DiemDen,
                    TenTau = l.Tau!.TenTau,
                    TongSoGhe = _context.Ghes.Count(g => g.MaTau == l.MaTau),
                    SoGheTrong = _context.Ghes.Count(g => g.MaTau == l.MaTau)
                                 - _context.Ves.Count(v => v.MaLichTrinh == l.MaLichTrinh && v.TrangThai != "Đã hủy")
                })
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

            ViewBag.TongSoGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == lichTrinh.MaTau);
            var soVeDaDat = await _context.Ves.CountAsync(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy");
            ViewBag.SoGheTrongThucTe = (int)ViewBag.TongSoGheThucTe - soVeDaDat;

            return View(lichTrinh);
        }

        #endregion

        #region CREATE

        public IActionResult Create()
        {
            var vm = new LichTrinhViewModel
            {
                NgayGioKhoiHanh = DateTime.Now.AddHours(1),
                NgayGioCapBenDuKien = DateTime.Now.AddHours(3)
            };
            LoadDropdownData(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichTrinhViewModel vm)
        {
            await ValidateLichTrinhBusiness(vm, isEdit: false);

            if (ModelState.IsValid)
            {
                try
                {
                    var soGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == vm.MaTau);

                    var lichTrinh = new LichTrinh
                    {
                        MaTuyen = vm.MaTuyen,
                        MaTau = vm.MaTau,
                        NgayGioKhoiHanh = vm.NgayGioKhoiHanh,
                        NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien,
                        GiaVeCoBan = vm.GiaVeCoBan,
                        TrangThai = "Sắp khởi hành",
                        SoGheTrong = soGheThucTe
                    };

                    _context.Add(lichTrinh);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Tạo lịch trình", "LichTrinhs",
                        $"Tạo lịch trình mới ID: {lichTrinh.MaLichTrinh}. Khởi hành: {lichTrinh.NgayGioKhoiHanh}");

                    TempData["SuccessMessage"] = "Thêm lịch trình mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi tạo lịch trình", "LichTrinhs", ex.Message, "Error");
                    ModelState.AddModelError("", "Đã xảy ra lỗi hệ thống.");
                }
            }
            LoadDropdownData(vm);
            return View(vm);
        }

        #endregion

        #region EDIT

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs.FindAsync(id);
            if (lichTrinh == null) return NotFound();

            var tongGhe = await _context.Ghes.CountAsync(g => g.MaTau == lichTrinh.MaTau);
            var veDaDat = await _context.Ves.CountAsync(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy");

            var viewModel = new LichTrinhViewModel
            {
                MaLichTrinh = lichTrinh.MaLichTrinh,
                MaTuyen = lichTrinh.MaTuyen,
                MaTau = lichTrinh.MaTau,
                NgayGioKhoiHanh = lichTrinh.NgayGioKhoiHanh,
                NgayGioCapBenDuKien = lichTrinh.NgayGioCapBenDuKien,
                GiaVeCoBan = lichTrinh.GiaVeCoBan,
                TrangThai = lichTrinh.TrangThai,
                TongSoGhe = tongGhe,
                SoGheTrong = tongGhe - veDaDat,
                DanhSachTuyen = new SelectList(_context.TuyenDuongs, "MaTuyen", "TenTuyen", lichTrinh.MaTuyen),
                DanhSachTau = new SelectList(_context.Taus.Where(t => t.TrangThai == true || t.MaTau == lichTrinh.MaTau), "MaTau", "TenTau", lichTrinh.MaTau)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichTrinhViewModel vm)
        {
            if (id != vm.MaLichTrinh) return NotFound();

            var lichTrinhDb = await _context.LichTrinhs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLichTrinh == id);
            if (lichTrinhDb == null) return NotFound();

            await ValidateLichTrinhBusiness(vm, isEdit: true, lichTrinhDb: lichTrinhDb);

            if (ModelState.IsValid)
            {
                try
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                    if (lichTrinh == null) return NotFound();

                    // Logic ghi log thay đổi thời gian nếu đã bán vé
                    bool daCoVe = await _context.Ves.AnyAsync(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy");
                    if (daCoVe && vm.NgayGioKhoiHanh != lichTrinhDb.NgayGioKhoiHanh)
                    {
                        await GhiLogHeThong("Thay đổi giờ chạy", "LichTrinhs",
                            $"ID: {id}. Giờ cũ: {lichTrinhDb.NgayGioKhoiHanh} -> Giờ mới: {vm.NgayGioKhoiHanh}. Cảnh báo: Lịch này đã có khách đặt vé!", "Warning");
                    }

                    lichTrinh.MaTuyen = vm.MaTuyen;
                    lichTrinh.MaTau = vm.MaTau;
                    lichTrinh.NgayGioKhoiHanh = vm.NgayGioKhoiHanh;
                    lichTrinh.NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien;
                    lichTrinh.GiaVeCoBan = vm.GiaVeCoBan;
                    lichTrinh.TrangThai = vm.TrangThai;

                    _context.Update(lichTrinh);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật lịch trình", "LichTrinhs", $"Cập nhật thành công lịch trình ID: {id}");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi cập nhật lịch trình", "LichTrinhs", ex.Message, "Error");
                }
            }

            LoadDropdownData(vm);
            return View(vm);
        }

        #endregion

        #region DELETE

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (canDelete, message) = await CanDeleteLichTrinh(id);

            if (!canDelete)
                return Json(new { success = false, message = message });

            try
            {
                var lt = await _context.LichTrinhs.FindAsync(id);
                if (lt != null)
                {
                    _context.LichTrinhs.Remove(lt);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Xóa lịch trình", "LichTrinhs", $"Đã xóa vĩnh viễn lịch trình ID: {id}", "Warning");

                    return Json(new { success = true, message = "Lịch trình đã được xóa vĩnh viễn." });
                }
                return Json(new { success = false, message = "Dữ liệu không tồn tại." });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi xóa lịch trình", "LichTrinhs", ex.Message, "Error");
                return Json(new { success = false, message = "Lỗi hệ thống khi xóa." });
            }
        }

        #endregion

        #region PRIVATE LOGIC & HELPERS

        private async Task ValidateLichTrinhBusiness(LichTrinhViewModel vm, bool isEdit = false, LichTrinh? lichTrinhDb = null)
        {
            var bayGio = DateTime.Now;

            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");

            if (!isEdit && vm.NgayGioKhoiHanh < bayGio.AddMinutes(-5))
                ModelState.AddModelError("NgayGioKhoiHanh", "Thời gian khởi hành không được ở trong quá khứ!");

            var lichTrinhBiTrung = await _context.LichTrinhs
                .Where(l => l.MaTau == vm.MaTau && l.MaLichTrinh != vm.MaLichTrinh && l.TrangThai != "Đã hủy" && l.TrangThai != "Hoàn thành")
                .FirstOrDefaultAsync(l => vm.NgayGioKhoiHanh < l.NgayGioCapBenDuKien && l.NgayGioKhoiHanh < vm.NgayGioCapBenDuKien);

            if (lichTrinhBiTrung != null)
                ModelState.AddModelError("MaTau", $"Trùng lịch chạy của tàu này (ID trùng: {lichTrinhBiTrung.MaLichTrinh}).");

            var tau = await _context.Taus.AsNoTracking().FirstOrDefaultAsync(t => t.MaTau == vm.MaTau);
            if (tau != null)
            {
                if (!tau.TrangThai) ModelState.AddModelError("MaTau", "Tàu này không sẵn sàng.");
                var soGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == vm.MaTau);
                if (soGheThucTe <= 0) ModelState.AddModelError("MaTau", "Tàu chưa có cấu hình ghế.");
            }

            if (isEdit && lichTrinhDb != null)
            {
                bool daCoVe = await _context.Ves.AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh && v.TrangThai != "Đã hủy");
                if (daCoVe)
                {
                    if (vm.MaTau != lichTrinhDb.MaTau) ModelState.AddModelError("MaTau", "Đã bán vé, không được đổi tàu!");
                    if (vm.GiaVeCoBan != lichTrinhDb.GiaVeCoBan) ModelState.AddModelError("GiaVeCoBan", "Đã bán vé, không được đổi giá!");
                }
            }
        }

        private async Task<(bool canDelete, string message)> CanDeleteLichTrinh(int id)
        {
            var lichTrinh = await _context.LichTrinhs.FindAsync(id);
            if (lichTrinh == null) return (false, "Không tìm thấy lịch trình.");
            if (lichTrinh.TrangThai == "Đang vận hành" || lichTrinh.TrangThai == "Hoàn thành")
                return (false, "Lịch trình đang chạy hoặc đã xong không thể xóa.");
            bool daCoVe = await _context.Ves.AnyAsync(v => v.MaLichTrinh == id);
            if (daCoVe) return (false, "Đã có khách đặt vé, không thể xóa!");
            return (true, "");
        }

        private void LoadDropdownData(LichTrinhViewModel vm)
        {
            vm.DanhSachTuyen = _context.TuyenDuongs.Select(t => new SelectListItem { Value = t.MaTuyen.ToString(), Text = t.TenTuyen });
            var queryTau = _context.Taus.Where(t => t.TrangThai == true || t.MaTau == vm.MaTau)
                .Select(t => new { t.MaTau, t.TenTau, SoGhe = _context.Ghes.Count(g => g.MaTau == t.MaTau) }).ToList();

            vm.DanhSachTau = queryTau.Select(t => new SelectListItem
            {
                Value = t.MaTau.ToString(),
                Text = t.SoGhe > 0 ? $"{t.TenTau} ({t.SoGhe} ghế)" : $"⚠️ {t.TenTau} (CHƯA CÓ GHẾ)"
            }).ToList();
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