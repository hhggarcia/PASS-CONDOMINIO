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
    public class MonedaCuentasController : Controller
    {
        private readonly PruebaContext _context;

        public MonedaCuentasController(PruebaContext context)
        {
            _context = context;
        }

        // GET: MonedaCuentas
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.MonedaCuenta.Include(m => m.IdCodCuentaNavigation).Include(m => m.IdMonedaNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        // GET: MonedaCuentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MonedaCuenta == null)
            {
                return NotFound();
            }

            var monedaCuenta = await _context.MonedaCuenta
                .Include(m => m.IdCodCuentaNavigation)
                .Include(m => m.IdMonedaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monedaCuenta == null)
            {
                return NotFound();
            }

            return View(monedaCuenta);
        }

        // GET: MonedaCuentas/Create
        public IActionResult Create()
        {
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "IdMoneda");
            return View();
        }

        // POST: MonedaCuentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCodCuenta,IdMoneda,RecibePagos")] MonedaCuenta monedaCuenta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(monedaCuenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", monedaCuenta.IdCodCuenta);
            ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "IdMoneda", monedaCuenta.IdMoneda);
            return View(monedaCuenta);
        }

        // GET: MonedaCuentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MonedaCuenta == null)
            {
                return NotFound();
            }

            var monedaCuenta = await _context.MonedaCuenta.FindAsync(id);
            if (monedaCuenta == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", monedaCuenta.IdCodCuenta);
            ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "IdMoneda", monedaCuenta.IdMoneda);
            return View(monedaCuenta);
        }

        // POST: MonedaCuentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCodCuenta,IdMoneda,RecibePagos")] MonedaCuenta monedaCuenta)
        {
            if (id != monedaCuenta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monedaCuenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonedaCuentaExists(monedaCuenta.Id))
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
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", monedaCuenta.IdCodCuenta);
            ViewData["IdMoneda"] = new SelectList(_context.Moneda, "IdMoneda", "IdMoneda", monedaCuenta.IdMoneda);
            return View(monedaCuenta);
        }

        // GET: MonedaCuentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MonedaCuenta == null)
            {
                return NotFound();
            }

            var monedaCuenta = await _context.MonedaCuenta
                .Include(m => m.IdCodCuentaNavigation)
                .Include(m => m.IdMonedaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monedaCuenta == null)
            {
                return NotFound();
            }

            return View(monedaCuenta);
        }

        // POST: MonedaCuentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MonedaCuenta == null)
            {
                return Problem("Entity set 'PruebaContext.MonedaCuenta'  is null.");
            }
            var monedaCuenta = await _context.MonedaCuenta.FindAsync(id);
            if (monedaCuenta != null)
            {
                _context.MonedaCuenta.Remove(monedaCuenta);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonedaCuentaExists(int id)
        {
          return (_context.MonedaCuenta?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
