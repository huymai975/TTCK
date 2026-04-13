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

        public KhuyenMaisController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        #region READ (Index & Details)

        // GET: Admin/KhuyenMais
        public async Task<IActionResult> Index()
        {
            var bayGio = DateTime.Now;
            var listKhuyenMai = await _context.KhuyenMais.ToListAsync();
            bool coThayDoi = false;

            foreach (var km in listKhuyenMai)
            {
                var trangThaiGoc = km.TrangThai;

                // Nếu đã hủy thì giữ nguyên, không tự động cập nhật lại
                if (km.TrangThai == "Đã hủy") continue;

                if (bayGio < km.NgayBatDau)
                    km.TrangThai = "Sắp diễn ra";
                else if (bayGio >= km.NgayBatDau && bayGio <= km.NgayKetThuc)
                    km.TrangThai = "Đang diễn ra";
                else
                    km.TrangThai = "Đã kết thúc";

                if (trangThaiGoc != km.TrangThai) coThayDoi = true;
            }

            if (coThayDoi) await _context.SaveChangesAsync();

            return View(listKhuyenMai.OrderByDescending(k => k.NgayBatDau).ToList());
        }

        // GET: Admin/KhuyenMais/Details/5
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

        // GET: Admin/KhuyenMais/Create
        public IActionResult Create()
        {
            var model = new KhuyenMai
            {
                // Gợi ý mã tự động dựa trên Ticks để tránh trùng lặp ban đầu
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
            // 1. Kiểm tra nghiệp vụ ngày tháng
            ValidateKhuyenMaiBusiness(khuyenMai);

            // 2. Kiểm tra trùng mã
            if (await _context.KhuyenMais.AnyAsync(k => k.MaKM == khuyenMai.MaKM))
            {
                ModelState.AddModelError("MaKM", "Mã khuyến mãi này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 3. Xử lý Upload Ảnh
                    if (ImageFile != null)
                    {
                        khuyenMai.HinhAnh = await SaveImage(ImageFile);
                    }

                    _context.Add(khuyenMai);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm chương trình khuyến mãi thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu dữ liệu.");
                }
            }
            return View(khuyenMai);
        }

        #endregion

        #region EDIT

        // GET: Admin/KhuyenMais/Edit/5
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

            ValidateKhuyenMaiBusiness(khuyenMai);

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null)
                    {
                        // Lấy lại thông tin bản ghi cũ từ DB để lấy tên ảnh cũ
                        var oldData = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(x => x.MaKM == id);
                        if (oldData != null && !string.IsNullOrEmpty(oldData.HinhAnh))
                        {
                            DeleteOldImage(oldData.HinhAnh); // Xóa ảnh cũ
                        }

                        khuyenMai.HinhAnh = await SaveImage(ImageFile); // Lưu ảnh mới
                    }
                    else
                    {
                        // Nếu không có ảnh mới, báo EF không update cột HinhAnh
                        _context.Entry(khuyenMai).Property(x => x.HinhAnh).IsModified = false;
                    }

                    _context.Update(khuyenMai);
                    await _context.SaveChangesAsync();
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

        #region DELETE (AJAX)

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
                return Json(new { success = false, message = "Không tìm thấy dữ liệu." });

            try
            {
                // Thay vì xóa, ta cập nhật trạng thái
                khuyenMai.TrangThai = "Đã kết thúc";

                // Bạn có thể thêm một flag DaXoa = true nếu DB có cột này, 
                // hoặc đơn giản là dùng trạng thái như bạn yêu cầu.

                _context.Update(khuyenMai);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã chuyển trạng thái khuyến mãi sang 'Đã kết thúc'." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật dữ liệu." });
            }
        }

        #endregion

        #region HELPERS (Hàm bổ trợ)

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
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, @"images/promotions/");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }

        private bool KhuyenMaiExists(string id)
        {
            return _context.KhuyenMais.Any(e => e.MaKM == id);
        }

        #endregion
    }
}