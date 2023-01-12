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
    public class PuestoEsController : Controller
    {
        private readonly PruebaContext _context;

        public PuestoEsController(PruebaContext context)
        {
            _context = context;
        }

        // GET: PuestoEs
        public async Task<IActionResult> Index(int id)
        {
            var pruebaContext = _context.PuestoEs.Include(p => p.IdEstacionamientoNavigation).Include(p => p.IdPropiedadNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        // GET: PuestoEs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PuestoEs == null)
            {
                return NotFound();
            }

            var puestoE = await _context.PuestoEs
                .Include(p => p.IdEstacionamientoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPuestoE == id);
            if (puestoE == null)
            {
                return NotFound();
            }

            return View(puestoE);
        }

        // GET: PuestoEs/Create
        public IActionResult Create()
        {
            ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "IdEstacionamiento");
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad");
            return View();
        }

        // POST: PuestoEs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPuestoE,IdEstacionamiento,IdPropiedad,Codigo,Alicuota")] PuestoE puestoE)
        {
            if (ModelState.IsValid)
            {
                _context.Add(puestoE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "IdEstacionamiento", puestoE.IdEstacionamiento);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", puestoE.IdPropiedad);
            return View(puestoE);
        }

        // GET: PuestoEs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PuestoEs == null)
            {
                return NotFound();
            }

            var puestoE = await _context.PuestoEs.FindAsync(id);
            if (puestoE == null)
            {
                return NotFound();
            }
            ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "IdEstacionamiento", puestoE.IdEstacionamiento);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", puestoE.IdPropiedad);
            return View(puestoE);
        }

        // POST: PuestoEs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPuestoE,IdEstacionamiento,IdPropiedad,Codigo,Alicuota")] PuestoE puestoE)
        {
            if (id != puestoE.IdPuestoE)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(puestoE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PuestoEExists(puestoE.IdPuestoE))
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
            ViewData["IdEstacionamiento"] = new SelectList(_context.Estacionamientos, "IdEstacionamiento", "IdEstacionamiento", puestoE.IdEstacionamiento);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "IdPropiedad", puestoE.IdPropiedad);
            return View(puestoE);
        }

        // GET: PuestoEs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PuestoEs == null)
            {
                return NotFound();
            }

            var puestoE = await _context.PuestoEs
                .Include(p => p.IdEstacionamientoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPuestoE == id);
            if (puestoE == null)
            {
                return NotFound();
            }

            return View(puestoE);
        }

        // POST: PuestoEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PuestoEs == null)
            {
                return Problem("Entity set 'PruebaContext.PuestoEs'  is null.");
            }
            var puestoE = await _context.PuestoEs.FindAsync(id);
            if (puestoE != null)
            {
                _context.PuestoEs.Remove(puestoE);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PuestoEExists(int id)
        {
          return (_context.PuestoEs?.Any(e => e.IdPuestoE == id)).GetValueOrDefault();
        }
    }
}
