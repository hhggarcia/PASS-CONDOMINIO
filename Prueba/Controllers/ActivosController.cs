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
    public class ActivosController : Controller
    {
        private readonly NuevaAppContext _context;

        public ActivosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Activos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Activos.Include(a => a.IdAsientoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Activos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activo = await _context.Activos
                .Include(a => a.IdAsientoNavigation)
                .FirstOrDefaultAsync(m => m.IdActivo == id);
            if (activo == null)
            {
                return NotFound();
            }

            return View(activo);
        }

        // GET: Activos/Create
        public IActionResult Create()
        {
            ViewData["IdAsiento"] = new SelectList(_context.LdiarioGlobals, "IdAsiento", "IdAsiento");
            return View();
        }

        // POST: Activos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdActivo,IdAsiento")] Activo activo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(activo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdAsiento"] = new SelectList(_context.LdiarioGlobals, "IdAsiento", "IdAsiento", activo.IdAsiento);
            return View(activo);
        }

        // GET: Activos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activo = await _context.Activos.FindAsync(id);
            if (activo == null)
            {
                return NotFound();
            }
            ViewData["IdAsiento"] = new SelectList(_context.LdiarioGlobals, "IdAsiento", "IdAsiento", activo.IdAsiento);
            return View(activo);
        }

        // POST: Activos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdActivo,IdAsiento")] Activo activo)
        {
            if (id != activo.IdActivo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivoExists(activo.IdActivo))
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
            ViewData["IdAsiento"] = new SelectList(_context.LdiarioGlobals, "IdAsiento", "IdAsiento", activo.IdAsiento);
            return View(activo);
        }

        // GET: Activos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activo = await _context.Activos
                .Include(a => a.IdAsientoNavigation)
                .FirstOrDefaultAsync(m => m.IdActivo == id);
            if (activo == null)
            {
                return NotFound();
            }

            return View(activo);
        }

        // POST: Activos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activo = await _context.Activos.FindAsync(id);
            if (activo != null)
            {
                _context.Activos.Remove(activo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivoExists(int id)
        {
            return _context.Activos.Any(e => e.IdActivo == id);
        }
    }
}
