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
            var bayGio = DateTime.Now;

            // 1. Logic tự động cập nhật trạng thái dựa trên thời gian
            var lichTrinhsCheck = await _context.LichTrinhs
                .Where(l => l.TrangThai == "Sắp khởi hành" || l.TrangThai == "Đang vận hành")
                .ToListAsync();

            bool coThayDoi = false;
            foreach (var item in lichTrinhsCheck)
            {
                string trangThaiCu = item.TrangThai;

                if (bayGio >= item.NgayGioCapBenDuKien)
                {
                    item.TrangThai = "Hoàn thành";
                }
                else if (bayGio >= item.NgayGioKhoiHanh)
                {
                    item.TrangThai = "Đang vận hành";
                }

                if (trangThaiCu != item.TrangThai) coThayDoi = true;
            }

            if (coThayDoi)
            {
                await _context.SaveChangesAsync();
            }

            // 2. Lấy danh sách hiển thị
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
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

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
            var bayGio = DateTime.Now;

            // 1. Chặn thời gian trong quá khứ
            if (vm.NgayGioKhoiHanh < bayGio.AddMinutes(-5)) // Cho phép lệch 5p do độ trễ thao tác
            {
                ModelState.AddModelError("NgayGioKhoiHanh", "Thời gian khởi hành không được ở trong quá khứ!");
            }

            // 2. Chặn giờ cập bến trước giờ đi
            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
            {
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tau = await _context.Taus.FindAsync(vm.MaTau);
                    var lichTrinh = new LichTrinh
                    {
                        MaTuyen = vm.MaTuyen,
                        MaTau = vm.MaTau,
                        NgayGioKhoiHanh = vm.NgayGioKhoiHanh,
                        NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien,
                        GiaVeCoBan = vm.GiaVeCoBan,
                        TrangThai = vm.TrangThai ?? "Sắp khởi hành",
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

            // 1. Lấy dữ liệu cũ từ DB (dùng AsNoTracking để không bị xung đột khi FindAsync phía dưới)
            var lichTrinhDb = await _context.LichTrinhs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLichTrinh == id);
            if (lichTrinhDb == null) return NotFound();

            // 2. RÀNG BUỘC THỜI GIAN & TRẠNG THÁI

            // Kiểm tra giờ cập bến phải sau giờ khởi hành (Lỗi CK_LT_ThoiGian ní gặp lúc nãy)
            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
            {
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");
            }

            // Nếu trạng thái TRONG DATABASE hiện tại đã là Đang vận hành/Hoàn thành thì KHÔNG cho sửa giờ
            if (lichTrinhDb.TrangThai != "Sắp khởi hành")
            {
                if (vm.NgayGioKhoiHanh != lichTrinhDb.NgayGioKhoiHanh || vm.NgayGioCapBenDuKien != lichTrinhDb.NgayGioCapBenDuKien)
                {
                    ModelState.AddModelError("", "Không cho phép thay đổi thời gian của chuyến đi đã hoặc đang vận hành!");
                }
            }

            // Nếu chuyến đi đang là "Sắp khởi hành" thì không được sửa giờ khởi hành về quá khứ
            if (vm.TrangThai == "Sắp khởi hành" && vm.NgayGioKhoiHanh < DateTime.Now.AddMinutes(-1))
            {
                ModelState.AddModelError("NgayGioKhoiHanh", "Không được đổi thời gian khởi hành về quá khứ!");
            }

            // 3. XỬ LÝ LƯU DỮ LIỆU
            if (ModelState.IsValid)
            {
                try
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                    if (lichTrinh == null) return NotFound();

                    // Nếu đổi tàu, cập nhật lại số ghế trống dựa trên tàu mới
                    if (lichTrinh.MaTau != vm.MaTau)
                    {
                        var tauMoi = await _context.Taus.FindAsync(vm.MaTau);
                        // Lưu ý: Chỉ nên đổi ghế trống nếu chưa có ai đặt vé, 
                        // nhưng tạm thời theo logic của ní là reset theo tàu mới:
                        lichTrinh.SoGheTrong = tauMoi?.TongSoGhe ?? 0;
                    }

                    // Cập nhật thông tin
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
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LichTrinhs.Any(e => e.MaLichTrinh == id)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại dropdown để người dùng sửa lại
            LoadDropdownData(vm);
            return View(vm);
        }

        // GET: Admin/LichTrinhs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

            return View(lichTrinh);
        }


        // POST: Admin/LichTrinhs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                if (lichTrinh != null)
                {
                    // Ràng buộc logic: Nếu chuyến đi đã hoàn thành hoặc đang chạy thì không nên cho xóa
                    // Ní có thể bắt lỗi logic này trước khi xóa thực tế
                    if (lichTrinh.TrangThai == "Đang vận hành")
                    {
                        TempData["ErrorMessage"] = "Không thể xóa lịch trình đã hoặc đang vận hành.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.LichTrinhs.Remove(lichTrinh);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa lịch trình thành công!";
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để debug nếu cần
                Console.WriteLine(ex.Message);

                // Bắt lỗi khóa ngoại (ví dụ: đã có Vé đặt cho lịch trình này)
                TempData["ErrorMessage"] = "Không thể xóa lịch trình này. Nguyên nhân có thể do lịch trình đã có khách hàng đặt vé hoặc dữ liệu đang được sử dụng ở bảng khác.";
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
                Text = $"{t.TenTau} (Sức chứa: {t.TongSoGhe})"
            });
        }
        #endregion
    }
}