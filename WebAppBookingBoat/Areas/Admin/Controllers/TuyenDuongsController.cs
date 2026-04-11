using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TuyenDuongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TuyenDuongsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/TuyenDuongs
        public async Task<IActionResult> Index()
        {
            return View(await _context.TuyenDuongs.ToListAsync());
        }

        // GET: Admin/TuyenDuongs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/TuyenDuongs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TuyenDuongViewModel vm)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = "default-route.jpg";

                if (vm.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tuyen-duong");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var tuyenDuong = new TuyenDuong
                {
                    TenTuyen = vm.TenTuyen,
                    DiemDi = vm.DiemDi,
                    DiemDen = vm.DiemDen,
                    KhoangCach = vm.KhoangCach,
                    ThoiGianDuKien = vm.ThoiGianDuKien,
                    HinhAnh = uniqueFileName
                };

                _context.Add(tuyenDuong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới tuyến đường thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Admin/TuyenDuongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tìm tuyến đường theo ID
            var tuyenDuong = await _context.TuyenDuongs
                .FirstOrDefaultAsync(m => m.MaTuyen == id);

            if (tuyenDuong == null)
            {
                return NotFound();
            }

            return View(tuyenDuong);
        }


        // GET: Admin/TuyenDuongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tuyenDuong = await _context.TuyenDuongs.FindAsync(id);
            if (tuyenDuong == null) return NotFound();

            // Chuyển đổi từ Model sang ViewModel để hiển thị lên Form
            var vm = new TuyenDuongViewModel
            {
                MaTuyen = tuyenDuong.MaTuyen,
                TenTuyen = tuyenDuong.TenTuyen,
                DiemDi = tuyenDuong.DiemDi,
                DiemDen = tuyenDuong.DiemDen,
                KhoangCach = tuyenDuong.KhoangCach,
                ThoiGianDuKien = tuyenDuong.ThoiGianDuKien,
                HinhAnhCu = tuyenDuong.HinhAnh
            };

            return View(vm);
        }


        // POST: Admin/TuyenDuongs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TuyenDuongViewModel vm)
        {
            if (id != vm.MaTuyen) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var tuyenDuong = await _context.TuyenDuongs.FindAsync(id);
                    if (tuyenDuong == null) return NotFound();

                    tuyenDuong.TenTuyen = vm.TenTuyen;
                    tuyenDuong.DiemDi = vm.DiemDi;
                    tuyenDuong.DiemDen = vm.DiemDen;
                    tuyenDuong.KhoangCach = vm.KhoangCach;
                    tuyenDuong.ThoiGianDuKien = vm.ThoiGianDuKien;

                    if (vm.ImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tuyen-duong");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Xóa ảnh cũ nếu không phải ảnh default
                        if (!string.IsNullOrEmpty(tuyenDuong.HinhAnh) && tuyenDuong.HinhAnh != "default-route.jpg")
                        {
                            string oldPath = Path.Combine(uploadsFolder, tuyenDuong.HinhAnh);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await vm.ImageFile.CopyToAsync(fileStream);
                        }
                        tuyenDuong.HinhAnh = uniqueFileName;
                    }

                    _context.Update(tuyenDuong);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật tuyến đường thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TuyenDuongExists(vm.MaTuyen)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Admin/TuyenDuongs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tuyenDuong = await _context.TuyenDuongs
                .FirstOrDefaultAsync(m => m.MaTuyen == id);

            if (tuyenDuong == null) return NotFound();

            return View(tuyenDuong);
        }

        // POST: Admin/TuyenDuongs/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken] // Thêm bảo mật
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tuyen = await _context.TuyenDuongs.FindAsync(id);
            if (tuyen == null) return Json(new { success = false, message = "Không tìm thấy tuyến đường." });

            try
            {
                // 1. Lưu lại đường dẫn ảnh để xóa file vật lý
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tuyen-duong");
                string imagePath = Path.Combine(uploadsFolder, tuyen.HinhAnh ?? "");

                // 2. Xóa trong Database
                _context.TuyenDuongs.Remove(tuyen);
                await _context.SaveChangesAsync();

                // 3. Xóa file ảnh vật lý nếu không phải ảnh mặc định
                if (!string.IsNullOrEmpty(tuyen.HinhAnh) && tuyen.HinhAnh != "default-route.jpg")
                {
                    if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                }

                return Json(new { success = true, message = "Đã xóa tuyến đường và dữ liệu hình ảnh!" });
            }
            catch (Exception)
            {
                // Thường lỗi do ràng buộc khóa ngoại với bảng LichTrinh
                return Json(new { success = false, message = "Không thể xóa vì tuyến này đang có lịch trình hoạt động." });
            }
        }

        private bool TuyenDuongExists(int id)
        {
            return _context.TuyenDuongs.Any(e => e.MaTuyen == id);
        }
    }
}