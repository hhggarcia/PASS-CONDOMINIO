using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    public class AreaComunsController : Controller
    {
        private readonly PruebaContext _context;

        public AreaComunsController(PruebaContext context)
        {
            _context = context;
        }

        // GET: AreaComuns
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.AreaComuns.Include(a => a.IdInmuebleNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        // GET: AreaComuns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AreaComuns == null)
            {
                return NotFound();
            }

            var areaComun = await _context.AreaComuns
                .Include(a => a.IdInmuebleNavigation)
                .FirstOrDefaultAsync(m => m.IdArea == id);
            if (areaComun == null)
            {
                return NotFound();
            }

            return View(areaComun);
        }

        // GET: AreaComuns/Create
        public IActionResult Create()
        {
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble");
            return View();
        }

        // POST: AreaComuns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdArea,IdInmueble,Nombre")] AreaComun areaComun)
        {
            if (ModelState.IsValid)
            {
                _context.Add(areaComun);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", areaComun.IdInmueble);
            return View(areaComun);
        }

        // GET: AreaComuns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AreaComuns == null)
            {
                return NotFound();
            }

            var areaComun = await _context.AreaComuns.FindAsync(id);
            if (areaComun == null)
            {
                return NotFound();
            }
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", areaComun.IdInmueble);
            return View(areaComun);
        }

        // POST: AreaComuns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdArea,IdInmueble,Nombre")] AreaComun areaComun)
        {
            if (id != areaComun.IdArea)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(areaComun);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AreaComunExists(areaComun.IdArea))
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
            ViewData["IdInmueble"] = new SelectList(_context.Inmuebles, "IdInmueble", "IdInmueble", areaComun.IdInmueble);
            return View(areaComun);
        }

        // GET: AreaComuns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AreaComuns == null)
            {
                return NotFound();
            }

            var areaComun = await _context.AreaComuns
                .Include(a => a.IdInmuebleNavigation)
                .FirstOrDefaultAsync(m => m.IdArea == id);
            if (areaComun == null)
            {
                return NotFound();
            }

            return View(areaComun);
        }

        // POST: AreaComuns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AreaComuns == null)
            {
                return Problem("Entity set 'PruebaContext.AreaComuns'  is null.");
            }
            var areaComun = await _context.AreaComuns.FindAsync(id);
            if (areaComun != null)
            {
                _context.AreaComuns.Remove(areaComun);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AreaComunExists(int id)
        {
          return (_context.AreaComuns?.Any(e => e.IdArea == id)).GetValueOrDefault();
        }
    }
}
