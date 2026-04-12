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
            var applicationDbContext = _context.DanhGias.Include(d => d.Ve);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/DanhGias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGia = await _context.DanhGias
                .Include(d => d.Ve)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);
            if (danhGia == null)
            {
                return NotFound();
            }

            return View(danhGia);
        }

        // GET: Admin/DanhGias/Create
        public IActionResult Create()
        {
            ViewData["MaVe"] = new SelectList(_context.Ves, "MaVe", "TrangThai");
            return View();
        }

        // POST: Admin/DanhGias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDanhGia,MaVe,SoSao,NoiDung,NgayDanhGia,TrangThai")] DanhGia danhGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaVe"] = new SelectList(_context.Ves, "MaVe", "TrangThai", danhGia.MaVe);
            return View(danhGia);
        }

        // GET: Admin/DanhGias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return NotFound();
            }
            ViewData["MaVe"] = new SelectList(_context.Ves, "MaVe", "TrangThai", danhGia.MaVe);
            return View(danhGia);
        }

        // POST: Admin/DanhGias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDanhGia,MaVe,SoSao,NoiDung,NgayDanhGia,TrangThai")] DanhGia danhGia)
        {
            if (id != danhGia.MaDanhGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhGia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhGiaExists(danhGia.MaDanhGia))
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
            ViewData["MaVe"] = new SelectList(_context.Ves, "MaVe", "TrangThai", danhGia.MaVe);
            return View(danhGia);
        }

        // GET: Admin/DanhGias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGia = await _context.DanhGias
                .Include(d => d.Ve)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);
            if (danhGia == null)
            {
                return NotFound();
            }

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
                _context.DanhGias.Remove(danhGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhGiaExists(int id)
        {
            return _context.DanhGias.Any(e => e.MaDanhGia == id);
        }
    }
}
