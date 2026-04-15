using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhuyenMaisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<AppUser> _userManager;

        public KhuyenMaisController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        #region READ (Index & Details)

        public async Task<IActionResult> Index()
        {
            var bayGio = DateTime.Now;
            var listKhuyenMai = await _context.KhuyenMais.ToListAsync();
            bool coThayDoi = false;

            foreach (var km in listKhuyenMai)
            {
                var trangThaiGoc = km.TrangThai;

                if (km.TrangThai == "Đã hủy") continue;

                if (bayGio < km.NgayBatDau)
                    km.TrangThai = "Sắp diễn ra";
                else if (bayGio >= km.NgayBatDau && bayGio <= km.NgayKetThuc)
                    km.TrangThai = "Đang diễn ra";
                else
                    km.TrangThai = "Đã kết thúc";

                if (trangThaiGoc != km.TrangThai)
                {
                    coThayDoi = true;
                    await GhiLogHeThong("Hệ thống cập nhật trạng thái", "KhuyenMais",
                        $"Khuyến mãi {km.MaKM} tự động chuyển: {trangThaiGoc} -> {km.TrangThai}", "Info");
                }
            }

            if (coThayDoi) await _context.SaveChangesAsync();

            return View(listKhuyenMai.OrderByDescending(k => k.NgayBatDau).ToList());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var khuyenMai = await _context.KhuyenMais
                .FirstOrDefaultAsync(m => m.MaKM == id);

            if (khuyenMai == null) return NotFound();

            return View(khuyenMai);
        }

        #endregion

        #region CREATE

        public IActionResult Create()
        {
            var model = new KhuyenMai
            {
                MaKM = "KM" + DateTime.Now.Ticks.ToString().Substring(10),
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(7)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMai khuyenMai, IFormFile? ImageFile)
        {
            ValidateKhuyenMaiBusiness(khuyenMai);

            if (await _context.KhuyenMais.AnyAsync(k => k.MaKM == khuyenMai.MaKM))
            {
                ModelState.AddModelError("MaKM", "Mã khuyến mãi này đã tồn tại!");
            }

            var bayGio = DateTime.Now;

            // Ép kiểu trạng thái dựa trên ngày thực tế để tránh người dùng "hack" giao diện
            if (khuyenMai.TrangThai != "Đã hủy") // Không ghi đè nếu admin chủ động hủy
            {
                if (bayGio < khuyenMai.NgayBatDau)
                    khuyenMai.TrangThai = "Sắp diễn ra";
                else if (bayGio >= khuyenMai.NgayBatDau && bayGio <= khuyenMai.NgayKetThuc)
                    khuyenMai.TrangThai = "Đang diễn ra";
                else
                    khuyenMai.TrangThai = "Đã kết thúc";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null)
                    {
                        khuyenMai.HinhAnh = await SaveImage(ImageFile);
                    }

                    _context.Add(khuyenMai);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Thêm khuyến mãi", "KhuyenMais",
                        $"Tạo mới KM: {khuyenMai.TenChuongTrinh} (Mã: {khuyenMai.MaKM}) - Giảm: {khuyenMai.PhanTramGiam}%");

                    // SUCCESS MESSAGE
                    TempData["SuccessMessage"] = "Thêm chương trình khuyến mãi thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi thêm khuyến mãi", "KhuyenMais", ex.Message, "Error");
                    // ERROR MESSAGE
                    TempData["ErrorMessage"] = "Lỗi hệ thống khi lưu dữ liệu: " + ex.Message;
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu dữ liệu.");
                }
            }
            return View(khuyenMai);
        }

        #endregion

        #region EDIT

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null) return NotFound();

            return View(khuyenMai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KhuyenMai khuyenMai, IFormFile? ImageFile)
        {
            if (id != khuyenMai.MaKM) return NotFound();

            // Vẫn giữ validate nghiệp vụ cho Edit để tránh dữ liệu sai lệch khi sửa
            ValidateKhuyenMaiBusiness(khuyenMai);

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ để xử lý file ảnh
                    var existingKM = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(k => k.MaKM == id);

                    if (ImageFile != null)
                    {
                        // Xóa ảnh cũ trên server
                        if (!string.IsNullOrEmpty(existingKM?.HinhAnh))
                        {
                            DeleteOldImage(existingKM.HinhAnh);
                        }
                        // Lưu ảnh mới
                        khuyenMai.HinhAnh = await SaveImage(ImageFile);
                    }
                    else
                    {
                        // Người dùng không chọn ảnh mới -> giữ nguyên tên file cũ
                        khuyenMai.HinhAnh = existingKM?.HinhAnh;
                    }

                    _context.Update(khuyenMai);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật khuyến mãi", "KhuyenMais", $"Chỉnh sửa KM: {khuyenMai.MaKM}");

                    TempData["SuccessMessage"] = "Cập nhật thay đổi thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi cập nhật khuyến mãi", "KhuyenMais", ex.Message, "Error");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(khuyenMai);
        }

        #endregion

        #region DELETE (AJAX - Chuyển trạng thái)

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Tìm nhanh đối tượng
            var khuyenMai = await _context.KhuyenMais.FindAsync(id);

            if (khuyenMai == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chương trình khuyến mãi này." });
            }

            try
            {
                // Bỏ qua mọi logic kiểm tra ngày tháng hay điều kiện khác, ép buộc chuyển sang Đã hủy
                string trangThaiCu = khuyenMai.TrangThai;
                khuyenMai.TrangThai = "Đã hủy";

                _context.Update(khuyenMai);
                await _context.SaveChangesAsync();

                // Ghi log nhanh
                await GhiLogHeThong("Hủy khuyến mãi", "KhuyenMais",
                    $"Hủy: {khuyenMai.TenChuongTrinh} ({id}). Từ: {trangThaiCu}", "Warning");

                return Json(new { success = true, message = "Đã hủy chương trình khuyến mãi thành công." });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi hủy khuyến mãi", "KhuyenMais", ex.Message, "Error");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        #endregion

        #region HELPERS

        private void ValidateKhuyenMaiBusiness(KhuyenMai km)
        {
            if (km.NgayKetThuc <= km.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu!");
            }
        }

        private void DeleteOldImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "no-image.png") return;
            string path = Path.Combine(_hostEnvironment.WebRootPath, "images", "khuyen-mai", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            string folderPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "khuyen-mai");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(folderPath, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }

        private bool KhuyenMaiExists(string id) => _context.KhuyenMais.Any(e => e.MaKM == id);

        [NonAction]
        private async Task GhiLogHeThong(string hanhDong, string bang, string chiTiet, string loai = "Info")
        {
            var userId = _userManager.GetUserId(User);
            var log = new Log
            {
                MaTK = userId ?? "System", // Nếu chưa login thì ghi System
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