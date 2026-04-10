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

        // GET: Admin/LichTrinhs
        public async Task<IActionResult> Index()
        {
            var lichTrinhs = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .OrderByDescending(l => l.NgayGioKhoiHanh)
                .ToListAsync();
            return View(lichTrinhs);
        }

        // GET: Admin/LichTrinhs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Quan trọng: Phải dùng .Include để nạp dữ liệu từ bảng Tau và TuyenDuong
            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null)
            {
                return NotFound();
            }

            return View(lichTrinh);
        }

        // GET: Admin/LichTrinhs/Create
        public IActionResult Create()
        {
            var vm = new LichTrinhViewModel();
            LoadDropdownData(vm);
            return View(vm);
        }

        // POST: Admin/LichTrinhs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichTrinhViewModel vm)
        {
            // Kiểm tra logic thời gian
            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
            {
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin tàu để lấy tổng số ghế
                    var tau = await _context.Taus.FindAsync(vm.MaTau);

                    var lichTrinh = new LichTrinh
                    {
                        MaTuyen = vm.MaTuyen,
                        MaTau = vm.MaTau,
                        NgayGioKhoiHanh = vm.NgayGioKhoiHanh,
                        NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien,
                        GiaVeCoBan = vm.GiaVeCoBan,
                        TrangThai = vm.TrangThai,
                        // Tự động gán số ghế trống ban đầu = tổng số ghế của tàu
                        SoGheTrong = tau?.TongSoGhe ?? 0
                    };

                    _context.Add(lichTrinh);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm lịch trình mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Lỗi hệ thống khi lưu lịch trình.";
                }
            }
            LoadDropdownData(vm);
            return View(vm);
        }

        // GET: Admin/LichTrinhs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs.FindAsync(id);
            if (lichTrinh == null) return NotFound();

            var vm = new LichTrinhViewModel
            {
                MaLichTrinh = lichTrinh.MaLichTrinh,
                MaTau = lichTrinh.MaTau,
                MaTuyen = lichTrinh.MaTuyen,
                NgayGioKhoiHanh = lichTrinh.NgayGioKhoiHanh,
                NgayGioCapBenDuKien = lichTrinh.NgayGioCapBenDuKien,
                GiaVeCoBan = lichTrinh.GiaVeCoBan,
                TrangThai = lichTrinh.TrangThai
            };

            LoadDropdownData(vm);
            return View(vm);
        }

        // POST: Admin/LichTrinhs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichTrinhViewModel vm)
        {
            if (id != vm.MaLichTrinh) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                    if (lichTrinh == null) return NotFound();

                    // Nếu đổi tàu, cần cập nhật lại số ghế trống (Cẩn thận: thực tế nên kiểm tra xem có ai đặt vé chưa)
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
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật dữ liệu.";
                }
            }
            LoadDropdownData(vm);
            return View(vm);
        }

        // GET: Admin/LichTrinhs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin lịch trình kèm theo thông tin Tàu và Tuyến đường để hiển thị lên trang xác nhận
            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null)
            {
                return NotFound();
            }

            return View(lichTrinh);
        }

        // POST: Admin/LichTrinhs/Delete/5 (Xóa nhanh)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichTrinh = await _context.LichTrinhs.FindAsync(id);
            if (lichTrinh != null)
            {
                _context.LichTrinhs.Remove(lichTrinh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa lịch trình thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        #region Helpers
        private void LoadDropdownData(LichTrinhViewModel vm)
        {
            vm.DanhSachTuyen = _context.TuyenDuongs.Select(t => new SelectListItem
            {
                Value = t.MaTuyen.ToString(),
                Text = t.TenTuyen
            });

            vm.DanhSachTau = _context.Taus.Where(t => t.TrangThai).Select(t => new SelectListItem
            {
                Value = t.MaTau.ToString(),
                Text = t.TenTau + " (Sức chứa: " + t.TongSoGhe + ")"
            });
        }
        #endregion
    }
}