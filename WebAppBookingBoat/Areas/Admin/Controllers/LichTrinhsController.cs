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

        public LichTrinhsController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region READ (Index & Details)

        // GET: Admin/LichTrinhs
        public async Task<IActionResult> Index()
        {
            var bayGio = DateTime.Now;

            // 1. Tự động cập nhật trạng thái trước khi hiển thị
            var lichTrinhsUpdate = await _context.LichTrinhs
                .Where(l => l.TrangThai == "Sắp khởi hành" || l.TrangThai == "Đang vận hành")
                .ToListAsync();

            bool coThayDoi = false;
            foreach (var item in lichTrinhsUpdate)
            {
                var trangThaiGoc = item.TrangThai;
                if (bayGio >= item.NgayGioCapBenDuKien) item.TrangThai = "Hoàn thành";
                else if (bayGio >= item.NgayGioKhoiHanh) item.TrangThai = "Đang vận hành";

                if (trangThaiGoc != item.TrangThai) coThayDoi = true;
            }

            if (coThayDoi) await _context.SaveChangesAsync();

            // 2. Lấy dữ liệu và MAPPING sang ViewModel (Để fix lỗi CS1061)
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
                    // Gán dữ liệu cho các trường hiển thị
                    TenTuyen = l.TuyenDuong!.TenTuyen,
                    DiemDi = l.TuyenDuong!.DiemDi,
                    DiemDen = l.TuyenDuong!.DiemDen,
                    TenTau = l.Tau!.TenTau,
                    TongSoGhe = l.Tau!.TongSoGhe,
                    SoGheTrong = l.SoGheTrong
                })
                .ToListAsync();

            return View(list);
        }

        // GET: Admin/LichTrinhs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

            return View(lichTrinh);
        }

        #endregion

        #region CREATE

        // GET: Admin/LichTrinhs/Create
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
                var tau = await _context.Taus.FindAsync(vm.MaTau);
                var lichTrinh = new LichTrinh
                {
                    MaTuyen = vm.MaTuyen,
                    MaTau = vm.MaTau,
                    NgayGioKhoiHanh = vm.NgayGioKhoiHanh,
                    NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien,
                    GiaVeCoBan = vm.GiaVeCoBan,
                    TrangThai = "Sắp khởi hành",
                    SoGheTrong = tau?.TongSoGhe ?? 0
                };
                _context.Add(lichTrinh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm lịch trình mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            LoadDropdownData(vm);
            return View(vm);
        }

        #endregion

        #region EDIT

        // GET: Admin/LichTrinhs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lt = await _context.LichTrinhs.FindAsync(id);
            if (lt == null) return NotFound();

            var vm = new LichTrinhViewModel
            {
                MaLichTrinh = lt.MaLichTrinh,
                MaTau = lt.MaTau,
                MaTuyen = lt.MaTuyen,
                NgayGioKhoiHanh = lt.NgayGioKhoiHanh,
                NgayGioCapBenDuKien = lt.NgayGioCapBenDuKien,
                GiaVeCoBan = lt.GiaVeCoBan,
                TrangThai = lt.TrangThai
            };
            LoadDropdownData(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichTrinhViewModel vm)
        {
            if (id != vm.MaLichTrinh) return NotFound();

            var lichTrinhDb = await _context.LichTrinhs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLichTrinh == id);
            if (lichTrinhDb == null) return NotFound();

            await ValidateLichTrinhBusiness(vm, isEdit: true, lichTrinhDb: lichTrinhDb);

            if (ModelState.IsValid)
            {
                var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                if (lichTrinh == null) return NotFound();

                // Cập nhật lại số ghế nếu đổi tàu
                if (lichTrinh.MaTau != vm.MaTau)
                {
                    var tauMoi = await _context.Taus.FindAsync(vm.MaTau);
                    lichTrinh.SoGheTrong = tauMoi?.TongSoGhe ?? 0;
                }

                lichTrinh.MaTuyen = vm.MaTuyen;
                lichTrinh.MaTau = vm.MaTau;
                lichTrinh.NgayGioKhoiHanh = vm.NgayGioKhoiHanh;
                lichTrinh.NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien;
                lichTrinh.GiaVeCoBan = vm.GiaVeCoBan;
                lichTrinh.TrangThai = vm.TrangThai;

                _context.Update(lichTrinh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật lịch trình thành công!";
                return RedirectToAction(nameof(Index));
            }
            LoadDropdownData(vm);
            return View(vm);
        }

        #endregion

        #region DELETE

        // POST: Admin/LichTrinhs/Delete/5
        [HttpPost, ActionName("Delete")] // Giữ ActionName là Delete để khớp với AJAX url
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Kiểm tra nghiệp vụ (Hàm CanDeleteLichTrinh của Huy rất ổn)
            var (canDelete, message) = await CanDeleteLichTrinh(id);

            if (!canDelete)
            {
                // Trả về lỗi để SweetAlert hiện thông báo đỏ
                return Json(new { success = false, message = message });
            }

            try
            {
                var lt = await _context.LichTrinhs.FindAsync(id);
                if (lt != null)
                {
                    _context.LichTrinhs.Remove(lt);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Lịch trình đã được xóa vĩnh viễn." });
                }
                return Json(new { success = false, message = "Dữ liệu không tồn tại hoặc đã bị xóa trước đó." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: Không thể xóa lịch trình lúc này." });
            }
        }

        #endregion

        #region PRIVATE LOGIC & HELPERS

        private async Task ValidateLichTrinhBusiness(LichTrinhViewModel vm, bool isEdit = false, LichTrinh? lichTrinhDb = null)
        {
            var bayGio = DateTime.Now;

            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");

            if (vm.NgayGioKhoiHanh < bayGio.AddMinutes(-2))
                ModelState.AddModelError("NgayGioKhoiHanh", "Thời gian khởi hành không được ở trong quá khứ!");

            if (isEdit && lichTrinhDb != null)
            {
                bool daCoVe = await _context.Ves.AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh);
                if (daCoVe)
                {
                    if (vm.MaTau != lichTrinhDb.MaTau)
                        ModelState.AddModelError("MaTau", "Đã có khách đặt vé, không được phép thay đổi tàu!");
                    if (vm.GiaVeCoBan != lichTrinhDb.GiaVeCoBan)
                        ModelState.AddModelError("GiaVeCoBan", "Đã có khách đặt vé, không được thay đổi giá cơ bản!");
                }

                if (lichTrinhDb.TrangThai != "Sắp khởi hành")
                {
                    if (vm.NgayGioKhoiHanh != lichTrinhDb.NgayGioKhoiHanh || vm.NgayGioCapBenDuKien != lichTrinhDb.NgayGioCapBenDuKien)
                        ModelState.AddModelError("", "Không thể sửa thời gian khi chuyến đi đã hoặc đang chạy!");
                }
            }
        }

        private async Task<(bool canDelete, string message)> CanDeleteLichTrinh(int id)
        {
            var lichTrinh = await _context.LichTrinhs.FindAsync(id);
            if (lichTrinh == null) return (false, "Không tìm thấy lịch trình.");

            if (lichTrinh.TrangThai == "Đang vận hành" || lichTrinh.TrangThai == "Hoàn thành")
                return (false, "Không thể xóa lịch trình đang vận hành hoặc đã hoàn thành.");

            bool daCoVe = await _context.Ves.AnyAsync(v => v.MaLichTrinh == id);
            if (daCoVe) return (false, "Không thể xóa vì đã có khách hàng đặt vé!");

            return (true, "");
        }

        private void LoadDropdownData(LichTrinhViewModel vm)
        {
            vm.DanhSachTuyen = _context.TuyenDuongs.Select(t => new SelectListItem { Value = t.MaTuyen.ToString(), Text = t.TenTuyen });
            vm.DanhSachTau = _context.Taus.Where(t => t.TrangThai).Select(t => new SelectListItem
            {
                Value = t.MaTau.ToString(),
                Text = $"{t.TenTau} ({t.TongSoGhe} ghế)"
            });
        }

        #endregion
    }
}