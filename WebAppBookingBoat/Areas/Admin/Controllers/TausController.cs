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
            if (id == null)
            {
                return NotFound();
            }

            var tau = await _context.Taus
                .FirstOrDefaultAsync(m => m.MaTau == id);
            if (tau == null)
            {
                return NotFound();
            }

            return View(tau);
        }

        // GET: Admin/Taus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Taus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTau,TenTau,HinhAnh,TongSoGhe,SoGheThuong,SoGheVIP,TrangThai")] Tau tau)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tau);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tau);
        }

        // GET: Admin/Taus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tau = await _context.Taus.FindAsync(id);
            if (tau == null)
            {
                return NotFound();
            }
            return View(tau);
        }

        // POST: Admin/Taus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTau,TenTau,HinhAnh,TongSoGhe,SoGheThuong,SoGheVIP,TrangThai")] Tau tau)
        {
            if (id != tau.MaTau)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tau);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TauExists(tau.MaTau))
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
            return View(tau);
        }

        // GET: Admin/Taus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tau = await _context.Taus
                .FirstOrDefaultAsync(m => m.MaTau == id);
            if (tau == null)
            {
                return NotFound();
            }

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
                _context.Taus.Remove(tau);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TauExists(int id)
        {
            return _context.Taus.Any(e => e.MaTau == id);
        }
    }
}
