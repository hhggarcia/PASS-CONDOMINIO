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
    public class BonificacionsController : Controller
    {
        private readonly NuevaAppContext _context;

        public BonificacionsController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: Bonificacions
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Bonificaciones.Include(b => b.IdCodCuentaNavigation).Include(b => b.IdEmpleadoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Bonificacions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonificacion = await _context.Bonificaciones
                .Include(b => b.IdCodCuentaNavigation)
                .Include(b => b.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdBonificacion == id);
            if (bonificacion == null)
            {
                return NotFound();
            }

            return View(bonificacion);
        }

        // GET: Bonificacions/Create
        public IActionResult Create()
        {
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta");
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado");
            return View();
        }

        // POST: Bonificacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBonificacion,IdEmpleado,IdCodCuenta,Concepto,Monto,RefMonto,Activo")] Bonificacion bonificacion)
        {
            ModelState.Remove(nameof(bonificacion.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(bonificacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", bonificacion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado", bonificacion.IdEmpleado);
            return View(bonificacion);
        }

        // GET: Bonificacions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonificacion = await _context.Bonificaciones.FindAsync(id);
            if (bonificacion == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", bonificacion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado", bonificacion.IdEmpleado);
            return View(bonificacion);
        }

        // POST: Bonificacions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdBonificacion,IdEmpleado,IdCodCuenta,Concepto,Monto,RefMonto,Activo")] Bonificacion bonificacion)
        {
            if (id != bonificacion.IdBonificacion)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(bonificacion.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bonificacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonificacionExists(bonificacion.IdBonificacion))
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
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", bonificacion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "IdEmpleado", bonificacion.IdEmpleado);
            return View(bonificacion);
        }

        // GET: Bonificacions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonificacion = await _context.Bonificaciones
                .Include(b => b.IdCodCuentaNavigation)
                .Include(b => b.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdBonificacion == id);
            if (bonificacion == null)
            {
                return NotFound();
            }

            return View(bonificacion);
        }

        // POST: Bonificacions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bonificacion = await _context.Bonificaciones.FindAsync(id);
            if (bonificacion != null)
            {
                _context.Bonificaciones.Remove(bonificacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BonificacionExists(int id)
        {
            return _context.Bonificaciones.Any(e => e.IdBonificacion == id);
        }
    }
}
