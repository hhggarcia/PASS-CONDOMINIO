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

    public class DeduccionesController : Controller
    {
        private readonly NuevaAppContext _context;

        public DeduccionesController(NuevaAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Mostrar las deducciones de un empleado especifico
        /// </summary>
        /// <param name="id">Id del empleado</param>
        /// <returns></returns>
        public async Task<IActionResult> VerDeducciones(int id)
        {
            var deduccionesEmpleado = await _context.Deducciones.Where(c => c.IdEmpleado == id).Include(p => p.IdEmpleadoNavigation).ToListAsync();

            if (deduccionesEmpleado == null)
            {
                return NotFound();
            }

            return View("Index", deduccionesEmpleado);
        }

        // GET: Deducciones
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Deducciones.Include(d => d.IdEmpleadoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Deducciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deduccion = await _context.Deducciones
                .Include(d => d.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdDeduccion == id);
            if (deduccion == null)
            {
                return NotFound();
            }

            return View(deduccion);
        }

        // GET: Deducciones/Create
        public IActionResult Create()
        {
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula");
            return View();
        }

        // POST: Deducciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDeduccion,Concepto,Monto,RefMonto,Activo,IdEmpleado")] Deduccion deduccion)
        {
            ModelState.Remove(nameof(deduccion.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(deduccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula", deduccion.IdEmpleado);
            return View(deduccion);
        }

        // GET: Deducciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deduccion = await _context.Deducciones.FindAsync(id);
            if (deduccion == null)
            {
                return NotFound();
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula", deduccion.IdEmpleado);
            return View(deduccion);
        }

        // POST: Deducciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDeduccion,Concepto,Monto,RefMonto,Activo,IdEmpleado")] Deduccion deduccion)
        {
            if (id != deduccion.IdDeduccion)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(deduccion.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deduccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeduccionExists(deduccion.IdDeduccion))
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula", deduccion.IdEmpleado);
            return View(deduccion);
        }

        // GET: Deducciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deduccion = await _context.Deducciones
                .Include(d => d.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdDeduccion == id);
            if (deduccion == null)
            {
                return NotFound();
            }

            return View(deduccion);
        }

        // POST: Deducciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deduccion = await _context.Deducciones.FindAsync(id);
            if (deduccion != null)
            {
                _context.Deducciones.Remove(deduccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeduccionExists(int id)
        {
            return _context.Deducciones.Any(e => e.IdDeduccion == id);
        }
    }
}
