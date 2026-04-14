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
                if (trangThaiGoc != item.TrangThai)
                {
                    await GhiLogHeThong("Hệ thống cập nhật trạng thái", "LichTrinhs",
                        $"Lịch trình {item.MaLichTrinh} tự động chuyển: {trangThaiGoc} -> {item.TrangThai}");
                    coThayDoi = true;
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
            // 1. Gọi logic kiểm tra nghiệp vụ (Trùng lịch, thời gian, trạng thái tàu, thiết lập ghế)
            await ValidateLichTrinhBusiness(vm, isEdit: false);

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Lấy số lượng ghế thực tế từ bảng Ghes của con tàu được chọn
                    // Điều này đảm bảo SoGheTrong ban đầu khớp hoàn toàn với thiết lập tàu
                    var soGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == vm.MaTau);

                    // 3. Mapping dữ liệu từ ViewModel sang Model
                    var lichTrinh = new LichTrinh
                    {
                        MaTuyen = vm.MaTuyen,
                        MaTau = vm.MaTau,
                        NgayGioKhoiHanh = vm.NgayGioKhoiHanh,
                        NgayGioCapBenDuKien = vm.NgayGioCapBenDuKien,
                        GiaVeCoBan = vm.GiaVeCoBan,
                        TrangThai = "Sắp khởi hành", // Trạng thái mặc định khi mới tạo
                        SoGheTrong = soGheThucTe      // Khởi tạo số ghế trống bằng tổng số ghế hiện có
                    };

                    // 4. Lưu vào Database
                    _context.Add(lichTrinh);

                    await GhiLogHeThong("Tạo lịch trình", "LichTrinhs",
                        $"Tạo lịch trình mới ID: {lichTrinh.MaLichTrinh}. Khởi hành: {lichTrinh.NgayGioKhoiHanh}");

                    await _context.SaveChangesAsync();

                    // 5. Thông báo thành công (Dùng cho SweetAlert hoặc Toastr ở View Index)
                    TempData["SuccessMessage"] = "Thêm lịch trình mới thành công!";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi hệ thống khi lưu dữ liệu. Vui lòng thử lại.");
                }
            }

            // Nếu dữ liệu không hợp lệ (ModelState Invalid), load lại Dropdown để hiện lại View
            LoadDropdownData(vm);
            return View(vm);
        }

        private async Task ValidateLichTrinhBusiness(LichTrinhViewModel vm, bool isEdit = false, LichTrinh? lichTrinhDb = null)
        {
            var bayGio = DateTime.Now;

            // 1. Kiểm tra thời gian logic cơ bản
            if (vm.NgayGioCapBenDuKien <= vm.NgayGioKhoiHanh)
            {
                ModelState.AddModelError("NgayGioCapBenDuKien", "Thời gian cập bến phải sau thời gian khởi hành!");
            }

            if (!isEdit && vm.NgayGioKhoiHanh < bayGio.AddMinutes(-5))
            {
                ModelState.AddModelError("NgayGioKhoiHanh", "Thời gian khởi hành không được ở trong quá khứ!");
            }

            // 2. KIỂM TRA TRÙNG LỊCH TÀU
            var lichTrinhBiTrung = await _context.LichTrinhs
                .Where(l => l.MaTau == vm.MaTau && l.MaLichTrinh != vm.MaLichTrinh && l.TrangThai != "Đã hủy" && l.TrangThai != "Hoàn thành")
                .FirstOrDefaultAsync(l => vm.NgayGioKhoiHanh < l.NgayGioCapBenDuKien && l.NgayGioKhoiHanh < vm.NgayGioCapBenDuKien);

            if (lichTrinhBiTrung != null)
            {
                ModelState.AddModelError("MaTau", $"Trùng lịch! Tàu này đã có lịch chạy từ {lichTrinhBiTrung.NgayGioKhoiHanh:HH:mm dd/MM} đến {lichTrinhBiTrung.NgayGioCapBenDuKien:HH:mm dd/MM}.");
            }

            // 3. KIỂM TRA TRẠNG THÁI TÀU & THIẾT LẬP GHẾ
            var tau = await _context.Taus.AsNoTracking().FirstOrDefaultAsync(t => t.MaTau == vm.MaTau);
            if (tau != null)
            {
                // Kiểm tra sẵn sàng
                if (!tau.TrangThai)
                {
                    ModelState.AddModelError("MaTau", "Tàu này hiện đang bảo trì hoặc không sẵn sàng.");
                }

                // Kiểm tra số lượng ghế
                var soGheThucTe = await _context.Ghes.CountAsync(g => g.MaTau == vm.MaTau);
                if (soGheThucTe <= 0)
                {
                    ModelState.AddModelError("MaTau", "Tàu này chưa được thiết lập danh sách ghế! Vui lòng cấu hình ghế trước.");
                }
            }

            // 4. Logic khi Edit: Nếu đã bán vé
            if (isEdit && lichTrinhDb != null)
            {
                bool daCoVe = await _context.Ves.AnyAsync(v => v.MaLichTrinh == vm.MaLichTrinh && v.TrangThai != "Đã hủy");
                if (daCoVe)
                {
                    if (vm.MaTau != lichTrinhDb.MaTau)
                        ModelState.AddModelError("MaTau", "Đã có khách đặt vé, không được phép đổi tàu khác!");

                    if (vm.GiaVeCoBan != lichTrinhDb.GiaVeCoBan)
                        ModelState.AddModelError("GiaVeCoBan", "Đã có khách đặt vé, không được thay đổi giá vé!");

                    if (vm.NgayGioKhoiHanh != lichTrinhDb.NgayGioKhoiHanh)
                        TempData["WarningMessage"] = "Lưu ý: Bạn đang đổi giờ khởi hành của lịch trình đã có khách đặt vé!";
                }
            }
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

            // Lấy dữ liệu gốc từ DB để so sánh
            var lichTrinhDb = await _context.LichTrinhs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLichTrinh == id);
            if (lichTrinhDb == null) return NotFound();

            // GỌI VALIDATE TẠI ĐÂY
            await ValidateLichTrinhBusiness(vm, isEdit: true, lichTrinhDb: lichTrinhDb);

            if (ModelState.IsValid)
            {
                try
                {
                    var lichTrinh = await _context.LichTrinhs.FindAsync(id);
                    // ... (gán dữ liệu từ vm sang lichTrinh như code cũ của bạn) ...

                    _context.Update(lichTrinh!);

                    await GhiLogHeThong("Thay đổi giờ chạy", "LichTrinhs",
                            $"ID: {id}. Giờ cũ: {lichTrinhDb.NgayGioKhoiHanh} -> Giờ mới: {vm.NgayGioKhoiHanh}. Cảnh báo: Lịch này đã có khách đặt vé!", "Warning");

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException) { /* ... */ }
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

                    await GhiLogHeThong("Xóa lịch trình", "LichTrinhs", $"Đã xóa vĩnh viễn lịch trình ID: {id}", "Warning");

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
            vm.DanhSachTuyen = _context.TuyenDuongs
                .Select(t => new SelectListItem { Value = t.MaTuyen.ToString(), Text = t.TenTuyen });

            // Lấy danh sách tàu và đếm ghế ngay trong query để tối ưu hiệu năng
            var queryTau = _context.Taus
                .Where(t => t.TrangThai == true || t.MaTau == vm.MaTau) // Lấy tàu sẵn sàng hoặc tàu đang được chỉnh sửa
                .Select(t => new
                {
                    t.MaTau,
                    t.TenTau,
                    SoGhe = _context.Ghes.Count(g => g.MaTau == t.MaTau)
                }).ToList();

            if (!queryTau.Any())
            {
                vm.DanhSachTau = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "⚠️ Hiện không có tàu nào sẵn sàng" }
        };
            }
            else
            {
                vm.DanhSachTau = queryTau.Select(t => new SelectListItem
                {
                    Value = t.MaTau.ToString(),
                    // Nếu SoGhe = 0 thì hiện cảnh báo ngay trên tên tàu
                    Text = t.SoGhe > 0
                        ? $"{t.TenTau} ({t.SoGhe} ghế)"
                        : $"⚠️ {t.TenTau} (CHƯA CÓ GHẾ)",
                    // Bạn có thể cân nhắc Disabled tàu không có ghế nếu muốn
                    // Disabled = t.SoGhe <= 0 
                }).ToList();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableBoats(DateTime start, DateTime end, int? currentLichTrinhId = null)
        {
            var busyBoatIds = await _context.LichTrinhs
                .Where(l => l.MaLichTrinh != currentLichTrinhId && l.TrangThai != "Đã hủy")
                .Where(l => (start < l.NgayGioCapBenDuKien && end > l.NgayGioKhoiHanh))
                .Select(l => l.MaTau)
                .ToListAsync();

            var availableBoats = await _context.Taus
                .Where(t => t.TrangThai == true && !busyBoatIds.Contains(t.MaTau))
                // CHỈ LẤY TÀU CÓ GHẾ
                .Where(t => _context.Ghes.Any(g => g.MaTau == t.MaTau))
                .Select(t => new
                {
                    value = t.MaTau,
                    text = $"{t.TenTau} ({_context.Ghes.Count(g => g.MaTau == t.MaTau)} ghế)"
                })
                .ToListAsync();

            return Json(availableBoats);
        }

        #endregion
    }
}