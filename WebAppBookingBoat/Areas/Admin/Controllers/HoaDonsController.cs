using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBookingBoat.Models;
using WebAppBookingBoat.Repository;

namespace WebAppBookingBoat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaDonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/HoaDons
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.HoaDons.Include(h => h.KhachHang).Include(h => h.KhuyenMai).Include(h => h.NhanVien);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/HoaDons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.KhuyenMai)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Create
        public IActionResult Create()
        {
            ViewData["MaKH"] = new SelectList(_context.KhachHangs, "MaKH", "Email");
            ViewData["MaKM"] = new SelectList(_context.KhuyenMais, "MaKM", "MaKM");
            ViewData["MaNV"] = new SelectList(_context.NhanViens, "MaNV", "Email");
            return View();
        }

        // POST: Admin/HoaDons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaHoaDon,MaKH,MaNV,MaKM,NgayLap,SoLuongVe,TamTinh,SoTienGiam,TongTien,PhuongThucTT,TrangThai")] HoaDon hoaDon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hoaDon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKH"] = new SelectList(_context.KhachHangs, "MaKH", "Email", hoaDon.MaKH);
            ViewData["MaKM"] = new SelectList(_context.KhuyenMais, "MaKM", "MaKM", hoaDon.MaKM);
            ViewData["MaNV"] = new SelectList(_context.NhanViens, "MaNV", "Email", hoaDon.MaNV);
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }
            ViewData["MaKH"] = new SelectList(_context.KhachHangs, "MaKH", "Email", hoaDon.MaKH);
            ViewData["MaKM"] = new SelectList(_context.KhuyenMais, "MaKM", "MaKM", hoaDon.MaKM);
            ViewData["MaNV"] = new SelectList(_context.NhanViens, "MaNV", "Email", hoaDon.MaNV);
            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaHoaDon,MaKH,MaNV,MaKM,NgayLap,SoLuongVe,TamTinh,SoTienGiam,TongTien,PhuongThucTT,TrangThai")] HoaDon hoaDon)
        {
            if (id != hoaDon.MaHoaDon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoaDon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoaDonExists(hoaDon.MaHoaDon))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKH"] = new SelectList(_context.KhachHangs, "MaKH", "Email", hoaDon.MaKH);
            ViewData["MaKM"] = new SelectList(_context.KhuyenMais, "MaKM", "MaKM", hoaDon.MaKM);
            ViewData["MaNV"] = new SelectList(_context.NhanViens, "MaNV", "Email", hoaDon.MaNV);
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.KhuyenMai)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon != null)
            {
                _context.HoaDons.Remove(hoaDon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HoaDonExists(int id)
        {
            return _context.HoaDons.Any(e => e.MaHoaDon == id);
        }
    }
}
