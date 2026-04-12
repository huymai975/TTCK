using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VesController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region LOGIC TẬP TRUNG (Business Rules)

        private async Task ValidateVeBusiness(Ve ve, bool isEdit = false, string? trangThaiCu = null)
        {
            // 1. Kiểm tra lịch trình tồn tại
            var lichTrinh = await _context.LichTrinhs.FindAsync(ve.MaLichTrinh);
            if (lichTrinh == null)
            {
                ModelState.AddModelError("MaLichTrinh", "Lịch trình không tồn tại.");
                return;
            }

            // 2. Kiểm tra trùng ghế (Trừ trường hợp vé đã hủy)
            bool biTrungGhe = await _context.Ves.AnyAsync(v =>
                v.MaLichTrinh == ve.MaLichTrinh &&
                v.MaGhe == ve.MaGhe &&
                v.MaVe != ve.MaVe &&
                v.TrangThai != "Đã hủy");

            if (ve.TrangThai != "Đã hủy" && biTrungGhe)
            {
                ModelState.AddModelError("MaGhe", "Ghế này đã được đặt bởi một vé khác.");
            }

            // 3. Kiểm tra số lượng ghế trống khi tạo mới hoặc chuyển từ Hủy -> Hợp lệ
            if (ve.TrangThai != "Đã hủy" && (trangThaiCu == "Đã hủy" || !isEdit))
            {
                if (lichTrinh.SoGheTrong <= 0)
                {
                    ModelState.AddModelError("", "Lịch trình này đã hết chỗ ngồi.");
                }
            }
        }

        // Thêm hàm này vào khu vực #region LOGIC TẬP TRUNG
        private async Task TinhGiaVe(Ve ve)
        {
            // 1. Lấy thông tin Lịch trình kèm theo Tuyến đường để có giá gốc
            var lichTrinh = await _context.LichTrinhs
                .Include(lt => lt.TuyenDuong)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == ve.MaLichTrinh);

            // 2. Lấy thông tin Ghế để kiểm tra loại ghế
            var ghe = await _context.Ghes.FindAsync(ve.MaGhe);

            if (lichTrinh != null && lichTrinh.TuyenDuong != null && ghe != null)
            {
                // Lấy giá gốc từ cấu hình của Tuyến đường
                decimal giaGoc = lichTrinh.GiaVeCoBan;

                // 3. Logic tính toán: Nếu là ghế VIP thì tăng 20%
                if (ghe.LoaiGhe != null && ghe.LoaiGhe.Contains("VIP", StringComparison.OrdinalIgnoreCase))
                {
                    ve.GiaVe = giaGoc * 1.2m;
                }
                else
                {
                    ve.GiaVe = giaGoc;
                }
            }
        }

        private async Task UpdateSoGheTrong(Ve ve, bool isEdit, string trangThaiCu = null!, int? maLichTrinhCu = null)
        {
            // 1. XỬ LÝ CHO LỊCH TRÌNH MỚI (Lịch trình đang chọn trên vé)
            var lichTrinhMoi = await _context.LichTrinhs.Include(lt => lt.Tau)
                .FirstOrDefaultAsync(lt => lt.MaLichTrinh == ve.MaLichTrinh);

            if (!isEdit) // TRƯỜNG HỢP CREATE
            {
                if (ve.TrangThai != "Đã hủy" && lichTrinhMoi != null)
                {
                    if (lichTrinhMoi.SoGheTrong > 0) lichTrinhMoi.SoGheTrong--;
                }
            }
            else // TRƯỜNG HỢP EDIT
            {
                // A. Nếu Đổi Lịch Trình (Chuyển vé từ chuyến này sang chuyến khác)
                if (maLichTrinhCu.HasValue && maLichTrinhCu != ve.MaLichTrinh)
                {
                    // Trả lại 1 ghế cho lịch trình cũ (nếu vé cũ không phải là vé hủy)
                    if (trangThaiCu != "Đã hủy")
                    {
                        var ltCu = await _context.LichTrinhs.Include(lt => lt.Tau)
                            .FirstOrDefaultAsync(lt => lt.MaLichTrinh == maLichTrinhCu);
                        if (ltCu != null && ltCu.SoGheTrong < ltCu.Tau.TongSoGhe) ltCu.SoGheTrong++;
                    }

                    // Trừ 1 ghế ở lịch trình mới (nếu vé mới không phải là vé hủy)
                    if (ve.TrangThai != "Đã hủy" && lichTrinhMoi != null)
                    {
                        if (lichTrinhMoi.SoGheTrong > 0) lichTrinhMoi.SoGheTrong--;
                    }
                }
                // B. Nếu cùng Lịch trình nhưng chỉ đổi Trạng thái (Hợp lệ <-> Hủy)
                else if (trangThaiCu != ve.TrangThai && lichTrinhMoi != null)
                {
                    if (ve.TrangThai == "Đã hủy" && trangThaiCu != "Đã hủy")
                    {
                        if (lichTrinhMoi.SoGheTrong < lichTrinhMoi.Tau.TongSoGhe) lichTrinhMoi.SoGheTrong++;
                    }
                    else if (ve.TrangThai != "Đã hủy" && trangThaiCu == "Đã hủy")
                    {
                        if (lichTrinhMoi.SoGheTrong > 0) lichTrinhMoi.SoGheTrong--;
                    }
                }
            }
        }

        #endregion

        public async Task<IActionResult> Index()
        {
            var listVe = await _context.Ves
                .Include(v => v.Ghe)
                .Include(v => v.LichTrinh!).ThenInclude(lt => lt.Tau)
                .Include(v => v.LichTrinh!).ThenInclude(lt => lt.TuyenDuong)
                .Include(v => v.HoaDon!).ThenInclude(hd => hd.KhachHang)
                .Select(v => new VeViewModel
                {
                    MaVe = v.MaVe,
                    TenGhe = v.Ghe!.TenGhe,
                    LoaiGhe = v.Ghe!.LoaiGhe,
                    TenTau = v.LichTrinh!.Tau!.TenTau,
                    ThongTinChuyen = $"{v.LichTrinh!.TuyenDuong!.TenTuyen} ({v.LichTrinh!.NgayGioKhoiHanh:HH:mm dd/MM})",
                    TenKhachHang = v.HoaDon!.KhachHang!.HoTen,
                    MaHoaDon = v.MaHoaDon ?? 0,
                    GiaVe = v.GiaVe,
                    TrangThai = v.TrangThai,
                    // LẤY TRẠNG THÁI HÓA ĐƠN Ở ĐÂY
                    TrangThaiHoaDon = v.HoaDon != null ? v.HoaDon.TrangThai : ""
                })
                .OrderByDescending(v => v.MaVe)
                .ToListAsync();

            return View(listVe);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ve = await _context.Ves
                .Include(v => v.Ghe)
                .Include(v => v.LichTrinh).ThenInclude(lt => lt!.Tau)
                .Include(v => v.LichTrinh).ThenInclude(lt => lt!.TuyenDuong)
                .Include(v => v.HoaDon).ThenInclude(hd => hd!.KhachHang)
                .FirstOrDefaultAsync(m => m.MaVe == id);

            if (ve == null) return NotFound();

            // Mapping sang ViewModel
            var viewModel = new VeViewModel
            {
                MaVe = ve.MaVe,
                TenGhe = ve.Ghe?.TenGhe,
                LoaiGhe = ve.Ghe?.LoaiGhe,
                TenTau = ve.LichTrinh?.Tau?.TenTau,
                ThongTinChuyen = $"{ve.LichTrinh?.TuyenDuong?.TenTuyen} ({ve.LichTrinh?.NgayGioKhoiHanh:HH:mm dd/MM})",
                TenKhachHang = ve.HoaDon?.KhachHang?.HoTen,
                MaHoaDon = ve.MaHoaDon ?? 0,
                GiaVe = ve.GiaVe,
                TrangThai = ve.TrangThai,
                TrangThaiHoaDon = ve.HoaDon?.TrangThai
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Ve { TrangThai = "Đã thanh toán" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ve ve)
        {
            ModelState.Remove("HoaDon"); ModelState.Remove("Ghe"); ModelState.Remove("LichTrinh");

            // --- KIỂM TRA TRẠNG THÁI HÓA ĐƠN ---
            var hoaDon = await _context.HoaDons.FindAsync(ve.MaHoaDon);
            if (hoaDon != null && hoaDon.TrangThai == "Đã thanh toán")
            {
                ModelState.AddModelError("", "Hóa đơn này đã thanh toán, không thể thêm vé mới!");
                LoadDropdownData(ve);
                return View(ve);
            }

            await TinhGiaVe(ve);
            await ValidateVeBusiness(ve, isEdit: false);

            if (ModelState.IsValid)
            {
                await UpdateSoGheTrong(ve, isEdit: false, trangThaiCu: null!);

                _context.Add(ve);
                await _context.SaveChangesAsync();

                // --- CẬP NHẬT LẠI HÓA ĐƠN ---
                await UpdateHoaDonTongTien(ve.MaHoaDon ?? 0);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo vé thành công!";
                return RedirectToAction(nameof(Index));
            }
            LoadDropdownData(ve);
            return View(ve);
        }

        // GET: Admin/Ves/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ve = await _context.Ves.FindAsync(id);
            if (ve == null) return NotFound();

            LoadDropdownData(ve);
            return View(ve);
        }

        // POST: Admin/Ves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ve ve)
        {
            if (id != ve.MaVe) return NotFound();

            // 1. Lấy thông tin vé cũ (Sử dụng AsNoTracking để tránh xung đột khi Update)
            var veCu = await _context.Ves.AsNoTracking().FirstOrDefaultAsync(v => v.MaVe == id);
            if (veCu == null) return NotFound();

            // 2. KIỂM TRA HÓA ĐƠN: Nếu đã thanh toán thì KHÔNG cho sửa bất cứ gì của vé
            var hoaDon = await _context.HoaDons.FindAsync(ve.MaHoaDon);
            if (hoaDon?.TrangThai == "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa vé thuộc hóa đơn đã thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("HoaDon");
            ModelState.Remove("Ghe");
            ModelState.Remove("LichTrinh");

            // 3. Tính lại giá vé (phòng trường hợp đổi ghế thường -> VIP)
            await TinhGiaVe(ve);
            await ValidateVeBusiness(ve, isEdit: true, trangThaiCu: veCu.TrangThai);

            if (ModelState.IsValid)
            {
                try
                {
                    // 4. Cập nhật số ghế trống trên tàu
                    await UpdateSoGheTrong(ve, isEdit: true, trangThaiCu: veCu.TrangThai, maLichTrinhCu: veCu.MaLichTrinh);

                    _context.Update(ve);
                    await _context.SaveChangesAsync();

                    // 5. CẬP NHẬT LẠI TỔNG TIỀN VÀ SỐ LƯỢNG VÉ TRONG HÓA ĐƠN
                    await UpdateHoaDonTongTien(ve.MaHoaDon ?? 0);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật vé và hóa đơn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ves.Any(e => e.MaVe == ve.MaVe)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadDropdownData(ve);
            return View(ve);
        }


        // --- AJAX DELETE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ve = await _context.Ves.FindAsync(id);
            if (ve == null) return Json(new { success = false, message = "Không tìm thấy vé." });

            // 1. KIỂM TRA TRẠNG THÁI HÓA ĐƠN
            var hoaDon = await _context.HoaDons.FindAsync(ve.MaHoaDon);
            if (hoaDon?.TrangThai == "Đã thanh toán")
            {
                return Json(new { success = false, message = "Hóa đơn đã thanh toán, không cho phép xóa vé!" });
            }

            try
            {
                int maHoaDonTam = ve.MaHoaDon ?? 0;

                // 2. TRẢ LẠI GHẾ TRỐNG (Nếu vé đang xóa không phải là vé đã hủy)
                if (ve.TrangThai != "Đã hủy")
                {
                    var lichTrinh = await _context.LichTrinhs
                        .Include(lt => lt.Tau)
                        .FirstOrDefaultAsync(lt => lt.MaLichTrinh == ve.MaLichTrinh);

                    if (lichTrinh != null && lichTrinh.Tau != null)
                    {
                        // Chỉ cộng lại ghế nếu chưa đầy (an toàn dữ liệu)
                        if (lichTrinh.SoGheTrong < lichTrinh.Tau.TongSoGhe)
                        {
                            lichTrinh.SoGheTrong++;
                        }
                    }
                }

                // 3. XÓA VÉ
                _context.Ves.Remove(ve);
                await _context.SaveChangesAsync();

                // 4. CẬP NHẬT LẠI TỔNG TIỀN & SỐ LƯỢNG VÉ CỦA HÓA ĐƠN
                await UpdateHoaDonTongTien(maHoaDonTam);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa vé và cập nhật lại hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGhesByLichTrinh(int maLichTrinh)
        {
            // 1. Tìm lịch trình để biết tàu nào đang chạy chuyến này
            var lichTrinh = await _context.LichTrinhs.FindAsync(maLichTrinh);
            if (lichTrinh == null) return Json(new List<object>());

            // 2. Lấy danh sách ID các ghế đã được đặt cho lịch trình này (trừ các vé đã hủy)
            var gheDaDatIds = await _context.Ves
                .Where(v => v.MaLichTrinh == maLichTrinh && v.TrangThai != "Đã hủy")
                .Select(v => v.MaGhe)
                .ToListAsync();

            // 3. Lấy danh sách tất cả ghế của tàu đó mà KHÔNG nằm trong danh sách đã đặt
            var ghesTrong = await _context.Ghes
                .Where(g => g.MaTau == lichTrinh.MaTau && !gheDaDatIds.Contains(g.MaGhe))
                .Select(g => new
                {
                    value = g.MaGhe,
                    text = $"{g.TenGhe} ({g.LoaiGhe})"
                })
                .ToListAsync();

            return Json(ghesTrong);
        }

        [HttpGet]
        public async Task<JsonResult> GetGiaCoBanByLichTrinh(int maLichTrinh)
        {
            var lt = await _context.LichTrinhs
                .Include(l => l.TuyenDuong)
                .FirstOrDefaultAsync(l => l.MaLichTrinh == maLichTrinh);

            return Json(new { giaGoc = lt?.GiaVeCoBan ?? 0 });
        }

        #region Helpers
        private void LoadDropdownData(Ve? ve = null)
        {
            var lichTrinhs = _context.LichTrinhs.Include(l => l.Tau).Include(l => l.TuyenDuong)
                .Select(l => new
                {
                    l.MaLichTrinh,
                    Display = $"{l.Tau!.TenTau} - {l.TuyenDuong!.TenTuyen} ({l.NgayGioKhoiHanh:dd/MM HH:mm})"
                }).ToList();
            ViewData["MaLichTrinh"] = new SelectList(lichTrinhs, "MaLichTrinh", "Display", ve?.MaLichTrinh);

            var hoaDons = _context.HoaDons.Include(h => h.KhachHang)
                .Select(h => new { h.MaHoaDon, Display = $"HĐ{h.MaHoaDon} - {h.KhachHang!.HoTen}" }).ToList();
            ViewData["MaHoaDon"] = new SelectList(hoaDons, "MaHoaDon", "Display", ve?.MaHoaDon);

            if (ve != null && ve.MaLichTrinh > 0)
            {
                var lt = _context.LichTrinhs.Find(ve.MaLichTrinh);
                if (lt != null)
                {
                    var ghes = _context.Ghes.Where(g => g.MaTau == lt.MaTau)
                        .Select(g => new { g.MaGhe, Display = $"{g.TenGhe} ({g.LoaiGhe})" }).ToList();
                    ViewData["MaGhe"] = new SelectList(ghes, "MaGhe", "Display", ve.MaGhe);
                }
            }
        }

        private async Task UpdateHoaDonTongTien(int maHoaDon)
        {
            var hoaDon = await _context.HoaDons.FindAsync(maHoaDon);
            if (hoaDon != null)
            {
                // Lấy danh sách vé thực tế (không tính vé hủy)
                var tatCaVe = await _context.Ves
                    .Where(v => v.MaHoaDon == maHoaDon && v.TrangThai != "Đã hủy")
                    .ToListAsync();

                // Cập nhật số lượng vé
                hoaDon.SoLuongVe = tatCaVe.Count;

                // Tính Tạm tính (Sum của decimal luôn ra decimal)
                hoaDon.TamTinh = tatCaVe.Sum(v => v.GiaVe);

                // Tính Tổng tiền: Vì SoTienGiam của Huy không nullable nên dùng trực tiếp
                hoaDon.TongTien = hoaDon.TamTinh - hoaDon.SoTienGiam;

                // Đảm bảo không bị âm (Ví dụ giảm giá quá đà)
                if (hoaDon.TongTien < 0m) hoaDon.TongTien = 0m;

                _context.Update(hoaDon);
            }
        }

        #endregion
    }
}