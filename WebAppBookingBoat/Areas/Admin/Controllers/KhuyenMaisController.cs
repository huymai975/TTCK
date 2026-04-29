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
        private readonly UserManager<AppUser> _userManager;
        private readonly PhotoService _photoService;

        // URL ảnh mặc định cho Khuyến mãi trên Cloudinary
        private const string DefaultCloudPromoImg = "https://res.cloudinary.com/dzvcaq2xl/image/upload/v1/WebAppBookingBoat/default-promo.jpg";

        public KhuyenMaisController(ApplicationDbContext context,
                                     UserManager<AppUser> userManager,
                                     PhotoService photoService)
        {
            _context = context;
            _userManager = userManager;
            _photoService = photoService;
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

                // Cập nhật trạng thái tự động dựa trên thời gian
                CapNhatTrangThaiTheoNgay(km);

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
            if (string.IsNullOrEmpty(id)) return NotFound();

            var khuyenMai = await _context.KhuyenMais.FirstOrDefaultAsync(m => m.MaKM == id);

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
                NgayKetThuc = DateTime.Now.AddDays(7),
                TrangThai = "Sắp diễn ra"
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
                    // 1. Xử lý ảnh (Cloudinary)
                    khuyenMai.HinhAnh = DefaultCloudPromoImg; // Mặc định ban đầu
                    if (ImageFile != null)
                    {
                        var result = await _photoService.AddPhotoAsync(ImageFile, "KhuyenMai");
                        if (result.Error == null)
                        {
                            khuyenMai.HinhAnh = result.SecureUrl.AbsoluteUri;
                        }
                    }

                    // 2. Chốt trạng thái cuối cùng trước khi lưu
                    CapNhatTrangThaiTheoNgay(khuyenMai);

                    _context.Add(khuyenMai);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Thêm khuyến mãi", "KhuyenMais", $"Tạo mới KM: {khuyenMai.MaKM}");
                    TempData["SuccessMessage"] = "Thêm chương trình khuyến mãi thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi thêm khuyến mãi", "KhuyenMais", ex.Message, "Error");
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            return View(khuyenMai);
        }

        #endregion

        #region EDIT

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
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
                    var existingKM = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(k => k.MaKM == id);
                    if (existingKM == null) return NotFound();

                    // Xử lý ảnh trên Cloud
                    if (ImageFile != null)
                    {
                        // Xóa ảnh cũ trên Cloud (nếu không phải ảnh mặc định)
                        var oldPublicId = GetPublicIdFromUrl(existingKM.HinhAnh);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _photoService.DeletePhotoAsync(oldPublicId);
                        }

                        // Upload ảnh mới
                        var result = await _photoService.AddPhotoAsync(ImageFile, "KhuyenMai");
                        if (result.Error == null)
                        {
                            khuyenMai.HinhAnh = result.SecureUrl.AbsoluteUri;
                        }
                    }
                    else
                    {
                        khuyenMai.HinhAnh = existingKM.HinhAnh; // Giữ link cũ
                    }

                    CapNhatTrangThaiTheoNgay(khuyenMai);

                    _context.Update(khuyenMai);
                    await _context.SaveChangesAsync();

                    await GhiLogHeThong("Cập nhật khuyến mãi", "KhuyenMais", $"Chỉnh sửa KM: {khuyenMai.MaKM}");
                    TempData["SuccessMessage"] = "Cập nhật thay đổi thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await GhiLogHeThong("Lỗi cập nhật khuyến mãi", "KhuyenMais", ex.Message, "Error");
                    ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
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
            if (khuyenMai == null) return Json(new { success = false, message = "Không tìm thấy." });

            try
            {
                string trangThaiCu = khuyenMai.TrangThai;
                khuyenMai.TrangThai = "Đã hủy";

                _context.Update(khuyenMai);
                await _context.SaveChangesAsync();

                await GhiLogHeThong("Hủy khuyến mãi", "KhuyenMais", $"Hủy KM: {id}. Trạng thái cũ: {trangThaiCu}", "Warning");
                return Json(new { success = true, message = "Đã hủy chương trình khuyến mãi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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

        private void CapNhatTrangThaiTheoNgay(KhuyenMai km)
        {
            if (km.TrangThai == "Đã hủy") return;

            var bayGio = DateTime.Now;
            if (bayGio < km.NgayBatDau)
                km.TrangThai = "Sắp diễn ra";
            else if (bayGio >= km.NgayBatDau && bayGio <= km.NgayKetThuc)
                km.TrangThai = "Đang diễn ra";
            else
                km.TrangThai = "Đã kết thúc";
        }

        private string? GetPublicIdFromUrl(string? url)
        {
            if (string.IsNullOrEmpty(url) || !url.Contains("res.cloudinary.com") || url == DefaultCloudPromoImg)
                return null;

            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex != -1)
                {
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