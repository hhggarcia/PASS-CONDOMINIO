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

    public class PercepcionesController : Controller
    {
        private readonly NuevaAppContext _context;

        public PercepcionesController(NuevaAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Vista para ver solo las percepciones de un empleado
        /// </summary>
        /// <param name="id"> ID del empleado al cual consultar las percepciones</param>
        /// <returns></returns>
        public async Task<IActionResult> VerPercepciones(int id)
        {
            var percepcionesEmpleado = await _context.Percepciones.Where(c => c.IdEmpleado ==  id).Include(p => p.IdEmpleadoNavigation).ToListAsync();

            if (percepcionesEmpleado == null)
            {
                return NotFound();
            }

            return View("Index", percepcionesEmpleado);
        }

        // GET: Percepciones
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Percepciones.Include(p => p.IdEmpleadoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Percepciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var percepcion = await _context.Percepciones
                .Include(p => p.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdPercepcion == id);
            if (percepcion == null)
            {
                return NotFound();
            }

            return View(percepcion);
        }

        // GET: Percepciones/Create
        public IActionResult Create()
        {
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula");
            return View();
        }

        // POST: Percepciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPercepcion,Concepto,Monto,RefMonto,Activo,IdEmpleado")] Percepcion percepcion)
        {
            ModelState.Remove(nameof(percepcion.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(percepcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula", percepcion.IdEmpleado);
            return View(percepcion);
        }

        // GET: Percepciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var percepcion = await _context.Percepciones.FindAsync(id);
            if (percepcion == null)
            {
                return NotFound();
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula", percepcion.IdEmpleado);
            return View(percepcion);
        }

        // POST: Percepciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPercepcion,Concepto,Monto,RefMonto,Activo,IdEmpleado")] Percepcion percepcion)
        {
            if (id != percepcion.IdPercepcion)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(percepcion.IdEmpleadoNavigation));


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(percepcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PercepcionExists(percepcion.IdPercepcion))
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Cedula", percepcion.IdEmpleado);
            return View(percepcion);
        }

        // GET: Percepciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var percepcion = await _context.Percepciones
                .Include(p => p.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdPercepcion == id);
            if (percepcion == null)
            {
                return NotFound();
            }

            return View(percepcion);
        }

        // POST: Percepciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var percepcion = await _context.Percepciones.FindAsync(id);
            if (percepcion != null)
            {
                _context.Percepciones.Remove(percepcion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PercepcionExists(int id)
        {
            return _context.Percepciones.Any(e => e.IdPercepcion == id);
        }
    }
}
