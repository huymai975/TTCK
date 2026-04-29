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
    public class TuyenDuongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly PhotoService _photoService;

        // URL ảnh mặc định lưu trên Cloud của bạn (Thay URL này bằng link ảnh default trên Cloudinary của bạn)
        private const string DefaultCloudImageUrl = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1/WebAppBookingBoat/default-route.jpg";

        public TuyenDuongsController(ApplicationDbContext context,
                                     UserManager<AppUser> userManager,
                                     PhotoService photoService)
        {
            _context = context;
            _userManager = userManager;
            _photoService = photoService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.TuyenDuongs.OrderByDescending(x => x.MaTuyen).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                // Thay vì return NotFound() gây lỗi View, ta trả về trang danh sách kèm thông báo
                TempData["ErrorMessage"] = "Không tìm thấy mã tuyến đường.";
                return RedirectToAction(nameof(Index));
            }

            var tuyenDuong = await _context.TuyenDuongs
                .FirstOrDefaultAsync(m => m.MaTuyen == id);

            if (tuyenDuong == null)
            {
                TempData["ErrorMessage"] = "Tuyến đường không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            return View(tuyenDuong);
        }

        public IActionResult Create() => View(new TuyenDuongViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TuyenDuongViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Kiểm tra nghiệp vụ
            if (vm.DiemDi.Trim().Equals(vm.DiemDen.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("DiemDen", "Điểm đến không được trùng với điểm đi.");
                return View(vm);
            }

            if (await _context.TuyenDuongs.AnyAsync(t => t.TenTuyen == vm.TenTuyen))
            {
                ModelState.AddModelError("TenTuyen", "Tên tuyến đường này đã tồn tại.");
                return View(vm);
            }

            try
            {
                var tuyenDuong = new TuyenDuong
                {
                    TenTuyen = vm.TenTuyen,
                    DiemDi = vm.DiemDi,
                    DiemDen = vm.DiemDen,
                    KhoangCach = vm.KhoangCach,
                    ThoiGianDuKien = vm.ThoiGianDuKien,
                    HinhAnh = DefaultCloudImageUrl // Gán mặc định là link Cloud
                };

                // --- CLOUD UPLOAD ---
                if (vm.ImageFile != null)
                {
                    var result = await _photoService.AddPhotoAsync(vm.ImageFile, "TuyenDuongs");
                    if (result.Error == null)
                    {
                        tuyenDuong.HinhAnh = result.SecureUrl.AbsoluteUri;
                    }
                }

                _context.Add(tuyenDuong);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Thêm mới", "Tuyến đường", $"Tạo tuyến: {tuyenDuong.TenTuyen}");
                TempData["SuccessMessage"] = "Thêm mới tuyến đường thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await GhiLogHeThong("Lỗi Thêm mới", "Tuyến đường", ex.Message, "Error");
                ModelState.AddModelError("", "Đã xảy ra lỗi hệ thống khi lưu ảnh lên Cloud.");
                return View(vm);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tuyenDuong = await _context.TuyenDuongs.FindAsync(id);
            if (tuyenDuong == null) return NotFound();

            return View(new TuyenDuongViewModel
            {
                MaTuyen = tuyenDuong.MaTuyen,
                TenTuyen = tuyenDuong.TenTuyen,
                DiemDi = tuyenDuong.DiemDi,
                DiemDen = tuyenDuong.DiemDen,
                KhoangCach = tuyenDuong.KhoangCach,
                ThoiGianDuKien = tuyenDuong.ThoiGianDuKien,
                HinhAnhCu = tuyenDuong.HinhAnh
            });
        }

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

                    // --- CLOUD UPDATE ---
                    if (vm.ImageFile != null)
                    {
                        // 1. Xóa ảnh cũ trên Cloud (nếu không phải ảnh default)
                        var oldPublicId = GetPublicIdFromUrl(tuyenDuong.HinhAnh);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _photoService.DeletePhotoAsync(oldPublicId);
                        }

                        // 2. Upload ảnh mới
                        var result = await _photoService.AddPhotoAsync(vm.ImageFile, "TuyenDuongs");
                        if (result.Error == null)
                        {
                            tuyenDuong.HinhAnh = result.SecureUrl.AbsoluteUri;
                        }
                    }

                    _context.Update(tuyenDuong);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật", "Tuyến đường", $"ID: {id}");
                    TempData["SuccessMessage"] = "Cập nhật dữ liệu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi Cập nhật", "Tuyến đường", ex.Message, "Error");
                    ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu lên Cloud.");
                }
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAjax(int id)
        {
            var tuyen = await _context.TuyenDuongs.FindAsync(id);
            if (tuyen == null) return Json(new { success = false, message = "Không tìm thấy." });

            if (await _context.LichTrinhs.AnyAsync(l => l.MaTuyen == id))
                return Json(new { success = false, message = "Không thể xóa tuyến đang có lịch trình." });

            try
            {
                // --- CLOUD DELETE ---
                var publicId = GetPublicIdFromUrl(tuyen.HinhAnh);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _photoService.DeletePhotoAsync(publicId);
                }

                _context.TuyenDuongs.Remove(tuyen);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Xóa", "Tuyến đường", $"Tên: {tuyen.TenTuyen}", "Warning");
                return Json(new { success = true, message = "Đã xóa dữ liệu trên hệ thống và Cloud!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        #region Helpers
        private string? GetPublicIdFromUrl(string url)
        {
            // Nếu URL trống hoặc là ảnh mặc định (không nằm trong folder upload) thì không xóa
            if (string.IsNullOrEmpty(url) || !url.Contains("res.cloudinary.com") || url == DefaultCloudImageUrl)
                return null;

            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex != -1)
                {
                    // Cloudinary URL structure: /cloudname/image/upload/v1234567/Folder/filename.jpg
                    // Ta cần lấy: Folder/filename
                    var publicIdWithExtension = string.Join("/", segments.Skip(uploadIndex + 2));
                    return Path.ChangeExtension(publicIdWithExtension, null);
                }
            }
            catch { }
            return null;
        }

        [NonAction]
        private async Task GhiLogHeThong(string hanhDong, string bang, string chiTiet, string loai = "Info")
        {
            try
            {
                var log = new Log
                {
                    MaTK = _userManager.GetUserId(User) ?? "System",
                    HanhDong = hanhDong,
                    BangTacDong = bang,
                    NoiDungChiTiet = chiTiet,
                    LoaiLog = loai,
                    ThoiGian = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch { }
        }
        #endregion
    }
}