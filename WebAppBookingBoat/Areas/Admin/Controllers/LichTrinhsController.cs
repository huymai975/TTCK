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

            // 1. Tự động cập nhật trạng thái (Giữ nguyên của Huy)
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

                    // Đếm tổng số ghế thực tế từ bảng Ghes
                    TongSoGhe = _context.Ghes.Count(g => g.MaTau == l.MaTau),

                    // Ghế trống = Tổng ghế thực tế - Vé đã đặt (chưa hủy)
                    SoGheTrong = _context.Ghes.Count(g => g.MaTau == l.MaTau)
                                 - _context.Ves.Count(v => v.MaLichTrinh == l.MaLichTrinh && v.TrangThai != "Đã hủy")
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

            // --- TÍNH TOÁN CON SỐ THỰC TẾ TẠI ĐÂY ---
            // 1. Tổng số ghế thực tế có trong bảng Ghes của con tàu này
            ViewBag.TongSoGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == lichTrinh.MaTau);

            // 2. Số vé thực tế đã đặt (chưa hủy) cho lịch trình này
            var soVeDaDat = await _context.Ves.CountAsync(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy");

            // 3. Số ghế trống thực tế = Tổng ghế thực tế - Vé đã đặt
            ViewBag.SoGheTrongThucTe = (int)ViewBag.TongSoGheThucTe - soVeDaDat;

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
                // Đếm ghế thực tế để khởi tạo số ghế trống ban đầu
                var soGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == vm.MaTau);

                var lichTrinh = new LichTrinh
                {
                    MaTuyen = vm.MaTuyen,
                    MaTau = vm.MaTau,
                    NgayGioKhoiHanh = vm.NgayGioKhoiHanh,
                    NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien,
                    GiaVeCoBan = vm.GiaVeCoBan,
                    TrangThai = "Sắp khởi hành",
                    SoGheTrong = soGheThucTe // Lúc mới tạo, chưa có vé nên trống = tổng
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

            var lichTrinh = await _context.LichTrinhs
                .Include(l => l.Tau)
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(m => m.MaLichTrinh == id);

            if (lichTrinh == null) return NotFound();

            // Tính toán số ghế thực tế
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

                // Gán con số thực tế vào ViewModel
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

            if (ModelState.IsValid)
            {
                try
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                    if (lichTrinh == null) return NotFound();

                    // Cập nhật các thông tin cơ bản
                    lichTrinh.MaTuyen = vm.MaTuyen;
                    lichTrinh.NgayGioKhoiHanh = vm.NgayGioKhoiHanh;
                    lichTrinh.NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien;
                    lichTrinh.GiaVeCoBan = vm.GiaVeCoBan;
                    lichTrinh.TrangThai = vm.TrangThai;

                    // XỬ LÝ GHẾ TRỐNG KHI ĐỔI TÀU
                    // Luôn đếm lại để đảm bảo chính xác tuyệt đối
                    var tongGheTauMoi = await _context.Ghes.CountAsync(g => g.MaTau == vm.MaTau);
                    var veDaBan = await _context.Ves.CountAsync(v => v.MaLichTrinh == id && v.TrangThai != "Đã hủy");

                    lichTrinh.MaTau = vm.MaTau;
                    lichTrinh.SoGheTrong = tongGheTauMoi - veDaBan;

                    _context.Update(lichTrinh);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật lịch trình thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException) { /* Handle error */ }
            }
            // Load lại dữ liệu nếu lỗi...
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

            // 1. Kiểm tra thời gian logic cơ bản
            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");

            if (vm.NgayGioKhoiHanh < bayGio.AddMinutes(-2))
                ModelState.AddModelError("NgayGioKhoiHanh", "Thời gian khởi hành không được ở trong quá khứ!");

            // 2. KIỂM TRA TRÙNG LỊCH TÀU (Overlap Check)
            // Tìm các lịch trình khác của cùng con tàu này mà thời gian giao thoa với thời gian đang chọn
            var trungLich = await _context.LichTrinhs
                .AnyAsync(l => l.MaTau == vm.MaTau
                          && l.MaLichTrinh != vm.MaLichTrinh // Không tự kiểm tra chính nó khi Edit
                          && l.TrangThai != "Hoàn thành"    // Bỏ qua các chuyến đã xong
                          && l.NgayGioKhoiHanh < vm.NgayGioCapBenDuKien
                          && l.NgayGioCapBenDuKien > vm.NgayGioKhoiHanh);

            if (trungLich)
            {
                ModelState.AddModelError("MaTau", "Tàu này đã có lịch trình khác trong khoảng thời gian này!");
            }

            // 3. KIỂM TRA TRẠNG THÁI TÀU (Sẵn sàng hay Bảo trì)
            var tau = await _context.Taus.AsNoTracking().FirstOrDefaultAsync(t => t.MaTau == vm.MaTau);
            if (tau != null && !tau.TrangThai) // Giả sử TrangThai = true là Sẵn sàng
            {
                ModelState.AddModelError("MaTau", "Tàu này hiện đang bảo trì hoặc không sẵn sàng hoạt động!");
            }

            // 4. Các logic cũ về Vé đã đặt
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

            // Lấy danh sách tàu sẵn sàng
            var tàuReady = _context.Taus
                .Where(t => t.TrangThai == true)
                .Select(t => new SelectListItem
                {
                    Value = t.MaTau.ToString(),
                    // Hiển thị số ghế đếm từ bảng Ghes
                    Text = $"{t.TenTau} ({_context.Ghes.Count(g => g.MaTau == t.MaTau)} ghế thực tế)"
                }).ToList();

            if (!tàuReady.Any())
            {
                // Nếu không có tàu, tạo một item giả để thông báo
                vm.DanhSachTau = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "⚠️ Hiện không có tàu nào sẵn sàng" }
        };
            }
            else
            {
                vm.DanhSachTau = tàuReady;
            }

            //if (vm.MaTau <= 0)
            //{
            //    ModelState.AddModelError("MaTau", "Bạn không thể tạo lịch trình khi chưa chọn tàu!");
            //}
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableBoats(DateTime start, DateTime end, int? currentLichTrinhId = null)
        {
            // Lấy danh sách ID các tàu đã bận trong khoảng thời gian này
            var busyBoatIds = await _context.LichTrinhs
                .Where(l => l.MaLichTrinh != currentLichTrinhId && l.TrangThai != "Đã hủy")
                .Where(l => (start < l.NgayGioCapBenDuKien && end > l.NgayGioKhoiHanh))
                .Select(l => l.MaTau)
                .ToListAsync();

            var availableBoats = await _context.Taus
                .Where(t => t.TrangThai == true && !busyBoatIds.Contains(t.MaTau))
                .Select(t => new
                {
                    // Quan trọng: Tên thuộc tính phải là chữ thường hoặc khớp với JS
                    value = t.MaTau,
                    text = t.TenTau + " (" + _context.Ghes.Count(g => g.MaTau == t.MaTau) + " ghế)"
                })
                .ToListAsync();

            return Json(availableBoats); // Trả về dạng [{value: 1, text: "..."}, ...]
        }

        #endregion
    }
}