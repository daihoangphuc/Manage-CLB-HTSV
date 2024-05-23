using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using website_CLB_HTSV.Data;
using website_CLB_HTSV.Models;

namespace website_CLB_HTSV.Controllers
{
    public class ChucVusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChucVusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ChucVus
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Index()
        {
              return _context.ChucVu != null ? 
                          View(await _context.ChucVu.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.ChucVu'  is null.");
        }

        // GET: ChucVus/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ChucVu == null)
            {
                return NotFound();
            }

            var chucVu = await _context.ChucVu
                .FirstOrDefaultAsync(m => m.MaChucVu == id);
            if (chucVu == null)
            {
                return NotFound();
            }

            return View(chucVu);
        }

        // GET: ChucVus/Create
        [Authorize(Roles = "Administrators")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChucVus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Create([Bind("MaChucVu,TenChucVu")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chucVu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chucVu);
        }

        // GET: ChucVus/Edit/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ChucVu == null)
            {
                return NotFound();
            }

            var chucVu = await _context.ChucVu.FindAsync(id);
            if (chucVu == null)
            {
                return NotFound();
            }
            return View(chucVu);
        }

        // POST: ChucVus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("MaChucVu,TenChucVu")] ChucVu chucVu)
        {
            if (id != chucVu.MaChucVu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chucVu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChucVuExists(chucVu.MaChucVu))
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
            return View(chucVu);
        }

        // GET: ChucVus/Delete/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ChucVu == null)
            {
                return NotFound();
            }

            var chucVu = await _context.ChucVu
                .FirstOrDefaultAsync(m => m.MaChucVu == id);
            if (chucVu == null)
            {
                return NotFound();
            }

            return View(chucVu);
        }

        // POST: ChucVus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ChucVu == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ChucVu'  is null.");
            }
            var chucVu = await _context.ChucVu.FindAsync(id);
            if (chucVu != null)
            {
                _context.ChucVu.Remove(chucVu);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChucVuExists(string id)
        {
          return (_context.ChucVu?.Any(e => e.MaChucVu == id)).GetValueOrDefault();
        }
    }
}
