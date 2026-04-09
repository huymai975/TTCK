using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public TausController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Taus
        public async Task<IActionResult> Index()
        {
            return View(await _context.Taus.ToListAsync());
        }

        // GET: Admin/Taus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tau = await _context.Taus.FirstOrDefaultAsync(m => m.MaTau == id);
            if (tau == null) return NotFound();

            return View(tau);
        }

        // GET: Admin/Taus/Create
        public IActionResult Create()
        {
            return View(new TauViewModel());
        }

        // POST: Admin/Taus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TauViewModel vm)
        {
            // Kiểm tra ràng buộc logic ghế
            if (vm.TongSoGhe != (vm.SoGheThuong + vm.SoGheVIP))
            {
                ModelState.AddModelError("TongSoGhe", "Tổng số ghế không khớp (Thường + VIP)!");
            }

            if (ModelState.IsValid)
            {
                // Chuyển từ ViewModel sang Model thực
                var tau = new Tau
                {
                    TenTau = vm.TenTau,
                    SoGheThuong = vm.SoGheThuong,
                    SoGheVIP = vm.SoGheVIP,
                    TongSoGhe = vm.TongSoGhe,
                    TrangThai = vm.TrangThai,
                    HinhAnh = "default-boat.jpg" // Mặc định nếu không up ảnh
                };

                if (vm.ImageFile != null)
                {
                    tau.HinhAnh = await SaveImage(vm.ImageFile);
                }

                _context.Add(tau);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Admin/Taus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tau = await _context.Taus.FindAsync(id);
            if (tau == null) return NotFound();

            // Đổ dữ liệu từ Model vào ViewModel để hiển thị lên Form
            var vm = new TauViewModel
            {
                MaTau = tau.MaTau,
                TenTau = tau.TenTau,
                SoGheThuong = tau.SoGheThuong,
                SoGheVIP = tau.SoGheVIP,
                TrangThai = tau.TrangThai,
                HinhAnhCu = tau.HinhAnh
            };

            return View(vm);
        }

        // POST: Admin/Taus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TauViewModel vm)
        {
            if (id != vm.MaTau) return NotFound();

            if (vm.TongSoGhe != (vm.SoGheThuong + vm.SoGheVIP))
            {
                ModelState.AddModelError("TongSoGhe", "Tổng số ghế không khớp!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tau = await _context.Taus.FindAsync(id);
                    if (tau == null) return NotFound();

                    // Cập nhật thông tin từ ViewModel vào Model đã tìm thấy
                    tau.TenTau = vm.TenTau;
                    tau.SoGheThuong = vm.SoGheThuong;
                    tau.SoGheVIP = vm.SoGheVIP;
                    tau.TongSoGhe = vm.TongSoGhe;
                    tau.TrangThai = vm.TrangThai;

                    if (vm.ImageFile != null)
                    {
                        // Xóa ảnh cũ trước khi lưu ảnh mới (tránh rác server)
                        DeleteOldImage(tau.HinhAnh);
                        tau.HinhAnh = await SaveImage(vm.ImageFile);
                    }

                    _context.Update(tau);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TauExists(vm.MaTau)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Admin/Taus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tau = await _context.Taus.FirstOrDefaultAsync(m => m.MaTau == id);
            if (tau == null) return NotFound();

            return View(tau);
        }

        // POST: Admin/Taus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tau = await _context.Taus.FindAsync(id);
            if (tau != null)
            {
                DeleteOldImage(tau.HinhAnh); // Xóa ảnh khi xóa tàu
                _context.Taus.Remove(tau);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods (Các hàm bổ trợ)

        private async Task<string> SaveImage(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tau");

            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            string filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }

        private void DeleteOldImage(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default-boat.jpg") return;

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tau", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private bool TauExists(int id)
        {
            return _context.Taus.Any(e => e.MaTau == id);
        }

        #endregion
    }
}