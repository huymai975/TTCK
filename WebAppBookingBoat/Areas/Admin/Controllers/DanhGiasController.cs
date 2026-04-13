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

        public DanhGiasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DanhGias
        public async Task<IActionResult> Index()
        {
            // Lấy kèm thông tin Hóa đơn và Khách hàng để hiển thị ở danh sách
            var applicationDbContext = _context.DanhGias
                .Include(d => d.HoaDon)
                    .ThenInclude(h => h.KhachHang);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/DanhGias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var danhGia = await _context.DanhGias
                .Include(d => d.HoaDon)
                    .ThenInclude(h => h.KhachHang)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);

            if (danhGia == null) return NotFound();

            return View(danhGia);
        }

        // GET: Admin/DanhGias/Create
        public IActionResult Create()
        {
            // Hiển thị mã hóa đơn kèm tên khách hàng để Admin dễ chọn
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHoaDon"] = new SelectList(_context.HoaDons, "MaHoaDon", "MaHoaDon", danhGia.MaHoaDon);
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

            // Truyền thông tin hóa đơn ra để hiển thị (Read-only)
            ViewData["MaHoaDon"] = new SelectList(_context.HoaDons, "MaHoaDon", "MaHoaDon", danhGia.MaHoaDon);
            return View(danhGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhGia model)
        {
            if (id != model.MaDanhGia) return NotFound();

            // Truy vấn dữ liệu gốc từ DB
            var danhGiaInDb = await _context.DanhGias.FindAsync(id);
            if (danhGiaInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Chỉ cập nhật những trường Admin được phép sửa
                    danhGiaInDb.TrangThai = model.TrangThai;
                    danhGiaInDb.PhanHoiAdmin = model.PhanHoiAdmin;

                    // Tự động cập nhật ngày phản hồi
                    if (!string.IsNullOrEmpty(model.PhanHoiAdmin))
                    {
                        danhGiaInDb.NgayPhanHoi = DateTime.Now;
                    }

                    _context.Update(danhGiaInDb);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhGiaExists(model.MaDanhGia)) return NotFound();
                    else throw;
                }
            }
            return View(model);
        }

        //GET: Admin/DanhGias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhGia = await _context.DanhGias
                .Include(d => d.HoaDon)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);

            if (danhGia == null) return NotFound();

            return View(danhGia);
        }

        // POST: Admin/DanhGias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);

            if (danhGia != null)
            {
                // Thay vì xóa khỏi database, ta cập nhật trạng thái
                danhGia.TrangThai = "Đã ẩn";

                _context.Update(danhGia);
                await _context.SaveChangesAsync();

                // Nếu bạn gọi qua Ajax, nên trả về Ok() thay vì Redirect
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