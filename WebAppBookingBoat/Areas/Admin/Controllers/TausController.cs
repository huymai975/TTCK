using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TausController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TausController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Logic check trùng tên tàu
        private async Task<string?> CheckBusinessLogic(string tenTau, int? id = null)
        {
            if (await _context.Taus.AnyAsync(t => t.TenTau == tenTau && t.MaTau != id))
                return $"Tên tàu '{tenTau}' đã tồn tại trên hệ thống!";
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _context.Taus.OrderByDescending(t => t.MaTau).ToListAsync();
            return View(model);
        }

        public IActionResult Create() => View(new TauViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TauViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var error = await CheckBusinessLogic(vm.TenTau);
                if (error == null)
                {
                    try
                    {
                        var tau = new Tau
                        {
                            TenTau = vm.TenTau,
                            TongSoGhe = vm.TongSoGhe,
                            TrangThai = vm.TrangThai,
                            HinhAnh = "default-boat.jpg"
                        };
                        if (vm.ImageFile != null) tau.HinhAnh = await SaveImage(vm.ImageFile);

                        _context.Add(tau);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Thêm tàu thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch { error = "Lỗi hệ thống khi lưu dữ liệu."; }
                }
                TempData["ErrorMessage"] = error;
            }
            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tau = await _context.Taus.FindAsync(id);
            if (tau == null) return NotFound();

            return View(new TauViewModel
            {
                MaTau = tau.MaTau,
                TenTau = tau.TenTau,
                TongSoGhe = tau.TongSoGhe,
                TrangThai = tau.TrangThai,
                HinhAnhCu = tau.HinhAnh
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TauViewModel vm)
        {
            if (id != vm.MaTau) return NotFound();
            if (ModelState.IsValid)
            {
                var error = await CheckBusinessLogic(vm.TenTau, id);
                if (error == null)
                {
                    try
                    {
                        var tau = await _context.Taus.FindAsync(id);
                        if (tau == null) return NotFound();

                        tau.TenTau = vm.TenTau;
                        tau.TongSoGhe = vm.TongSoGhe;
                        tau.TrangThai = vm.TrangThai;

                        if (vm.ImageFile != null)
                        {
                            DeleteOldImage(tau.HinhAnh);
                            tau.HinhAnh = await SaveImage(vm.ImageFile);
                        }

                        _context.Update(tau);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch { error = "Lỗi khi cập nhật."; }
                }
                TempData["ErrorMessage"] = error;
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tau = await _context.Taus.FindAsync(id);
            if (tau == null) return Json(new { success = false, message = "Không tìm thấy tàu." });

            // Ràng buộc dữ liệu: Không xóa nếu có ghế hoặc lịch trình
            if (await _context.Ghes.AnyAsync(g => g.MaTau == id) || await _context.LichTrinhs.AnyAsync(l => l.MaTau == id))
                return Json(new { success = false, message = "Không thể xóa tàu đang có sơ đồ ghế hoặc lịch trình hoạt động." });

            try
            {
                DeleteOldImage(tau.HinhAnh);
                _context.Taus.Remove(tau);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa tàu thành công." });
            }
            catch { return Json(new { success = false, message = "Lỗi khi xóa dữ liệu." }); }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var tau = await _context.Taus.FindAsync(id);
            if (tau == null) return Json(new { success = false });

            tau.TrangThai = !tau.TrangThai;
            await _context.SaveChangesAsync();
            return Json(new { success = true, newState = tau.TrangThai });
        }

        #region Helpers
        private async Task<string> SaveImage(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tau");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }

        private void DeleteOldImage(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default-boat.jpg") return;
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tau", fileName);
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }
        #endregion
    }
}