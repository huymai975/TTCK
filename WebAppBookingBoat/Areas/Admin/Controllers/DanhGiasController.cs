using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhGiasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DanhGiasController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region PRIVATE LOGIC & HELPERS

        private async Task GhiLogHeThong(string hanhDong, string chiTiet, string loai = "Info")
        {
            var log = new Log
            {
                MaTK = _userManager.GetUserId(User),
                HanhDong = hanhDong,
                BangTacDong = "DanhGias",
                NoiDungChiTiet = chiTiet,
                LoaiLog = loai,
                ThoiGian = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        #endregion

        // GET: Admin/DanhGias
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DanhGias
                .Include(d => d.HoaDon)
                    .ThenInclude(h => h!.KhachHang);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/DanhGias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var danhGia = await _context.DanhGias
                .Include(d => d.HoaDon)
                    .ThenInclude(h => h!.KhachHang)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);

            if (danhGia == null) return NotFound();

            return View(danhGia);
        }

        // GET: Admin/DanhGias/Create
        public IActionResult Create()
        {
            var dsHoaDon = _context.HoaDons.Include(h => h.KhachHang).Select(h => new
            {
                MaHD = h.MaHoaDon,
                TenKH = h.MaHoaDon + " - " + (h.KhachHang != null ? h.KhachHang.HoTen : "N/A")
            });
            ViewData["MaHoaDon"] = new SelectList(dsHoaDon, "MaHD", "TenKH");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDanhGia,MaHoaDon,SoSao,NoiDung,HinhAnh,NgayDanhGia,TrangThai")] DanhGia danhGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhGia);
                await _context.SaveChangesAsync();
                await GhiLogHeThong("Tạo đánh giá", $"Tạo thủ công đánh giá cho Hóa đơn #{danhGia.MaHoaDon}");

                TempData["SuccessMessage"] = "Tạo đánh giá mới thành công!"; // Thêm dòng này
                return RedirectToAction(nameof(Index));
            }

            // Thêm dòng này để hiện lỗi Popup nếu Validation fail
            TempData["ErrorMessage"] = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(danhGia);
        }

        // GET: Admin/DanhGias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhGia = await _context.DanhGias
                .Include(d => d.HoaDon)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);

            if (danhGia == null) return NotFound();

            ViewData["MaHoaDon"] = new SelectList(_context.HoaDons, "MaHoaDon", "MaHoaDon", danhGia.MaHoaDon);
            return View(danhGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhGia model)
        {
            if (id != model.MaDanhGia) return NotFound();

            var danhGiaInDb = await _context.DanhGias.FirstOrDefaultAsync(d => d.MaDanhGia == id);
            if (danhGiaInDb == null) return NotFound();

            ModelState.Remove("HoaDon");
            ModelState.Remove("MaHoaDon");
            ModelState.Remove("SoSao");

            if (ModelState.IsValid)
            {
                try
                {
                    string oldStatus = danhGiaInDb.TrangThai;
                    bool isNewReply = string.IsNullOrEmpty(danhGiaInDb.PhanHoiAdmin) && !string.IsNullOrEmpty(model.PhanHoiAdmin);

                    danhGiaInDb.TrangThai = model.TrangThai;
                    danhGiaInDb.PhanHoiAdmin = model.PhanHoiAdmin;

                    if (!string.IsNullOrEmpty(model.PhanHoiAdmin))
                    {
                        danhGiaInDb.NgayPhanHoi = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    string actionNote = isNewReply ? "Phản hồi đánh giá" : "Cập nhật đánh giá";
                    await GhiLogHeThong(actionNote, $"ID: {id}. Trạng thái: {oldStatus} -> {model.TrangThai}");

                    TempData["SuccessMessage"] = "Cập nhật đánh giá và phản hồi thành công!"; // Thêm dòng này
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhGiaExists(model.MaDanhGia)) return NotFound();
                    else throw;
                }
            }

            // Thêm dòng này để hiện lỗi Popup
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại!";
            return View(danhGiaInDb);
        }

        // POST: Admin/DanhGias/Delete/5 (Ẩn đánh giá)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);

            if (danhGia != null)
            {
                danhGia.TrangThai = "Đã ẩn";
                _context.Update(danhGia);
                await _context.SaveChangesAsync();

                // Ghi Log: Ẩn đánh giá (Thường do nội dung nhạy cảm/không phù hợp)
                await GhiLogHeThong("Ẩn đánh giá", $"Ẩn đánh giá ID: {id} của Hóa đơn #{danhGia.MaHoaDon}", "Warning");

                return Json(new { success = true, message = "Đã ẩn đánh giá thành công." });
            }

            return Json(new { success = false, message = "Không tìm thấy dữ liệu." });
        }

        private bool DanhGiaExists(int id)
        {
            return _context.DanhGias.Any(e => e.MaDanhGia == id);
        }
    }
}