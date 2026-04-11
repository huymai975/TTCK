using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanViensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanViensController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/NhanViens
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.NhanViens.Include(n => n.AppUser).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.HoTen.Contains(searchString) || n.Email.Contains(searchString));
                ViewBag.Search = searchString;
            }

            return View(await query.ToListAsync());
        }

        // GET: Admin/NhanViens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .Include(n => n.AppUser)
                .Include(n => n.HoaDons) // Xem các hóa đơn nhân viên này đã lập
                .FirstOrDefaultAsync(m => m.MaNV == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Create
        public IActionResult Create()
        {
            // Hiển thị UserName thay vì Id để Admin dễ chọn tài khoản cho nhân viên
            ViewData["MaTK"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTK,HoTen,Sdt,Email,ChucVu,Luong,TrangThai")] Models.NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tài khoản này đã được gán cho nhân viên khác chưa (1-1)
                bool isUserTaken = await _context.NhanViens.AnyAsync(n => n.MaTK == nhanVien.MaTK);
                if (isUserTaken)
                {
                    ModelState.AddModelError("MaTK", "Tài khoản này đã được gán cho một nhân viên khác.");
                }
                else
                {
                    _context.Add(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["MaTK"] = new SelectList(_context.Users, "Id", "UserName", nhanVien.MaTK);
            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            ViewData["MaTK"] = new SelectList(_context.Users, "Id", "UserName", nhanVien.MaTK);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNV,MaTK,HoTen,Sdt,Email,ChucVu,Luong,TrangThai")] Models.NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật nhân viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNV)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaTK"] = new SelectList(_context.Users, "Id", "UserName", nhanVien.MaTK, nhanVien.MaTK);
            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .Include(n => n.AppUser)
                .FirstOrDefaultAsync(m => m.MaNV == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // POST: Admin/NhanViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return RedirectToAction(nameof(Index));

            // RÀNG BUỘC: Nhân viên đã lập hóa đơn thì không được xóa (để giữ lịch sử bán vé)
            bool hasInvoices = await _context.HoaDons.AnyAsync(h => h.MaNV == id);
            if (hasInvoices)
            {
                TempData["Error"] = "Không thể xóa nhân viên này vì đã có lịch sử lập hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa nhân viên.";
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(e => e.MaNV == id);
        }
    }
}