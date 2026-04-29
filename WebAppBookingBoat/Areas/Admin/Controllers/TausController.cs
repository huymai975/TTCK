using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;
using WebAppBookingBoat.ViewModels;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TausController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly PhotoService _photoService;

        public TausController(ApplicationDbContext context,
                              UserManager<AppUser> userManager,
                              PhotoService photoService)
        {
            _context = context;
            _userManager = userManager;
            _photoService = photoService;
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
                if (error != null)
                {
                    ModelState.AddModelError("TenTau", error);
                    return View(vm);
                }

                try
                {
                    var tau = new Tau
                    {
                        TenTau = vm.TenTau,
                        TongSoGhe = vm.TongSoGhe,
                        TrangThai = vm.TrangThai,
                        HinhAnh = "default-boat.jpg" // Ảnh mặc định nếu không upload
                    };

                    if (vm.ImageFile != null)
                    {
                        // Gọi service với folder "Taus"
                        var result = await _photoService.AddPhotoAsync(vm.ImageFile, "Taus");
                        if (result.Error == null)
                        {
                            tau.HinhAnh = result.SecureUrl.AbsoluteUri;
                        }
                    }

                    _context.Add(tau);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Thêm mới", "Tàu", $"Thêm tàu mới: {tau.TenTau}");
                    TempData["SuccessMessage"] = "Thêm tàu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi Thêm mới", "Tàu", ex.Message, "Error");
                }
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
                if (error != null)
                {
                    ModelState.AddModelError("TenTau", error);
                    return View(vm);
                }

                try
                {
                    var tau = await _context.Taus.FindAsync(id);
                    if (tau == null) return NotFound();

                    string tenCu = tau.TenTau;
                    tau.TenTau = vm.TenTau;
                    tau.TongSoGhe = vm.TongSoGhe;
                    tau.TrangThai = vm.TrangThai;

                    if (vm.ImageFile != null)
                    {
                        // 1. Xóa ảnh cũ trên Cloudinary để tiết kiệm dung lượng
                        var oldPublicId = GetPublicIdFromUrl(tau.HinhAnh);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _photoService.DeletePhotoAsync(oldPublicId);
                        }

                        // 2. Upload ảnh mới vào folder "Taus"
                        var result = await _photoService.AddPhotoAsync(vm.ImageFile, "Taus");
                        if (result.Error == null)
                        {
                            tau.HinhAnh = result.SecureUrl.AbsoluteUri;
                        }
                    }

                    _context.Update(tau);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật", "Tàu", $"Chỉnh sửa tàu ID {id}: '{tenCu}' -> '{tau.TenTau}'");
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi Cập nhật", "Tàu", ex.Message, "Error");
                }
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
            {
                await GhiLogHeThong("Xóa thất bại", "Tàu", $"Cố gắng xóa tàu ID {id} nhưng bị từ chối do có ràng buộc dữ liệu.", "Warning");
                return Json(new { success = false, message = "Không thể xóa tàu đang có sơ đồ ghế hoặc lịch trình hoạt động." });
            }

            try
            {
                // Xóa ảnh trên Cloudinary trước khi xóa tàu trong DB
                var publicId = GetPublicIdFromUrl(tau.HinhAnh);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _photoService.DeletePhotoAsync(publicId);
                }

                string thongTinTau = $"Xóa tàu: {tau.TenTau} (ID: {tau.MaTau})";
                _context.Taus.Remove(tau);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Xóa", "Tàu", thongTinTau, "Warning");
                return Json(new { success = true, message = "Đã xóa tàu thành công." });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi Xóa", "Tàu", $"ID: {id}. Lỗi: {ex.Message}", "Error");
                return Json(new { success = false, message = "Lỗi khi xóa dữ liệu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var tau = await _context.Taus.FindAsync(id);
            if (tau == null) return Json(new { success = false });

            try
            {
                tau.TrangThai = !tau.TrangThai;
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Thay đổi trạng thái", "Tàu", $"Tàu ID {id} sang {(tau.TrangThai ? "Hoạt động" : "Ngưng hoạt động")}");
                return Json(new { success = true, newState = tau.TrangThai });
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi ToggleStatus", "Tàu", $"ID: {id}. Lỗi: {ex.Message}", "Error");
                return Json(new { success = false });
            }
        }

        #region Helpers
        // Hàm trích xuất PublicId từ URL Cloudinary để thực hiện lệnh Xóa
        private string? GetPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || url == "default-boat.jpg" || !url.Contains("res.cloudinary.com"))
                return null;

            try
            {
                // URL: https://res.cloudinary.com/cloudname/image/upload/v12345/WebAppBookingBoat/Taus/filename.jpg
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                var uploadIndex = Array.IndexOf(segments, "upload");

                if (uploadIndex != -1)
                {
                    // Lấy tất cả các phần tử sau version (v12345)
                    var publicIdWithExtension = string.Join("/", segments.Skip(uploadIndex + 2));
                    // Bỏ phần mở rộng file (.jpg, .png...)
                    return Path.ChangeExtension(publicIdWithExtension, null);
                }
            }
            catch { }
            return null;
        }

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