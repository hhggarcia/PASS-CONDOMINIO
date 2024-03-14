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
using Prueba.Repositories;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class PercepcionesController : Controller
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public PercepcionesController(IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: Percepciones
        //public async Task<IActionResult> Index()
        //{
        //    var nuevaAppContext = _context.Percepciones.Include(p => p.IdEmpleadoNavigation);
        //    return View(await nuevaAppContext.ToListAsync());
        //}

        // GET: Percepciones
        public async Task<IActionResult> Index(int id)
        {
            var nuevaAppContext = _context.Percepciones.Include(p => p.IdEmpleadoNavigation).Where(c => c.IdEmpleado == id);
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre");
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
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                if (monedaPrincipal != null)
                {
                    percepcion.Monto = percepcion.RefMonto * monedaPrincipal.ValorDolar;
                }

                _context.Add(percepcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Empleados");
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", percepcion.IdEmpleado);
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", percepcion.IdEmpleado);
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
                    var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                    if (monedaPrincipal != null)
                    {
                        percepcion.Monto = percepcion.RefMonto * monedaPrincipal.ValorDolar;
                    }

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
                return RedirectToAction(nameof(Index), "Empleados");

            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", percepcion.IdEmpleado);
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
            return RedirectToAction(nameof(Index), "Empleados");
        }

        private bool PercepcionExists(int id)
        {
            return _context.Percepciones.Any(e => e.IdPercepcion == id);
        }
    }
}
