using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                    // Ghi log hệ thống tự cập nhật trạng thái
                    await GhiLogHeThong("Hệ thống cập nhật trạng thái", "KhuyenMais",
                        $"Khuyến mãi {km.MaKM} tự động chuyển: {trangThaiGoc} -> {km.TrangThai}", "Auto");
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

                    TempData["SuccessMessage"] = "Thêm chương trình khuyến mãi thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi thêm khuyến mãi", "KhuyenMais", ex.Message, "Error");
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

            var oldData = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(x => x.MaKM == id);
            if (oldData == null) return NotFound();

            ValidateKhuyenMaiBusiness(khuyenMai);

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(oldData.HinhAnh))
                        {
                            DeleteOldImage(oldData.HinhAnh);
                        }
                        khuyenMai.HinhAnh = await SaveImage(ImageFile);
                    }
                    else
                    {
                        khuyenMai.HinhAnh = oldData.HinhAnh; // Giữ lại ảnh cũ
                    }

                    _context.Update(khuyenMai);
                    await _context.SaveChangesAsync();

                    // Ghi log chi tiết nếu thay đổi mức giảm giá
                    string logDetail = $"Cập nhật KM: {khuyenMai.MaKM}.";
                    if (oldData.PhanTramGiam != khuyenMai.PhanTramGiam)
                        logDetail += $" Thay đổi giảm giá: {oldData.PhanTramGiam}% -> {khuyenMai.PhanTramGiam}%.";

                    await GhiLogHeThong("Cập nhật khuyến mãi", "KhuyenMais", logDetail);

                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhuyenMaiExists(khuyenMai.MaKM)) return NotFound();
                    else throw;
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
            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
                return Json(new { success = false, message = "Không tìm thấy dữ liệu." });

            try
            {
                string trangThaiCu = khuyenMai.TrangThai;
                khuyenMai.TrangThai = "Đã hủy";

                _context.Update(khuyenMai);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Hủy khuyến mãi", "KhuyenMais",
                    $"Hủy chương trình: {khuyenMai.TenChuongTrinh} ({id}). Trạng thái trước đó: {trangThaiCu}", "Warning");

                return Json(new { success = true, message = "Khuyến mãi đã được chuyển sang trạng thái 'Đã hủy'." });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi hủy khuyến mãi", "KhuyenMais", ex.Message, "Error");
                return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái dữ liệu." });
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
            string path = Path.Combine(_hostEnvironment.WebRootPath, "images/promotions/", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, @"images/promotions/");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }

        private bool KhuyenMaiExists(string id) => _context.KhuyenMais.Any(e => e.MaKM == id);

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