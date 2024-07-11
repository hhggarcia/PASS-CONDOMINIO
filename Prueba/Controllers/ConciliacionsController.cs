using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class ConciliacionsController : Controller
    {
        private readonly NuevaAppContext _context;

        public ConciliacionsController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Conciliacions
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Conciliacions.Include(c => c.IdCodCuentaNavigation).Include(c => c.IdCondominioNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Conciliacions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conciliacion = await _context.Conciliacions
                .Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdConciliacion == id);
            if (conciliacion == null)
            {
                return NotFound();
            }

            return View(conciliacion);
        }

        // GET: Conciliacions/Create
        public IActionResult Create()
        {
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            return View();
        }

        // POST: Conciliacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdConciliacion,IdCondominio,IdCodCuenta,FechaEmision,SaldoInicial,SaldoFinal,Actual,Activo,FechaInicio,FechaFin")] Conciliacion conciliacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(conciliacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", conciliacion.IdCodCuenta);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", conciliacion.IdCondominio);
            return View(conciliacion);
        }

        // GET: Conciliacions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conciliacion = await _context.Conciliacions.FindAsync(id);
            if (conciliacion == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", conciliacion.IdCodCuenta);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", conciliacion.IdCondominio);
            return View(conciliacion);
        }

        // POST: Conciliacions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdConciliacion,IdCondominio,IdCodCuenta,FechaEmision,SaldoInicial,SaldoFinal,Actual,Activo,FechaInicio,FechaFin")] Conciliacion conciliacion)
        {
            if (id != conciliacion.IdConciliacion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conciliacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConciliacionExists(conciliacion.IdConciliacion))
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
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", conciliacion.IdCodCuenta);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", conciliacion.IdCondominio);
            return View(conciliacion);
        }

        // GET: Conciliacions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conciliacion = await _context.Conciliacions
                .Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdConciliacion == id);
            if (conciliacion == null)
            {
                return NotFound();
            }

            return View(conciliacion);
        }

        // POST: Conciliacions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conciliacion = await _context.Conciliacions.FindAsync(id);
            if (conciliacion != null)
            {
                _context.Conciliacions.Remove(conciliacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConciliacionExists(int id)
        {
            return _context.Conciliacions.Any(e => e.IdConciliacion == id);
        }

        public IActionResult Conciliacion()
        {
            return View();
        }
    }
}
