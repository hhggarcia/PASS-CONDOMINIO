using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    public class FondosController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly NuevaAppContext _context;

        public FondosController(ICuentasContablesRepository repoCuentas,
            NuevaAppContext context)
        {
            _repoCuentas = repoCuentas;
            _context = context;
        }

        // GET: Fondos
        public async Task<IActionResult> Index()
        {
            var NuevaAppContext = _context.Fondos.Include(f => f.IdCodCuentaNavigation);
            return View(await NuevaAppContext.ToListAsync());
        }

        // GET: Fondos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Fondos == null)
            {
                return NotFound();
            }

            var fondo = await _context.Fondos
                .Include(f => f.IdCodCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdFondo == id);
            if (fondo == null)
            {
                return NotFound();
            }

            return RedirectToAction("RelaciondeGastos", "RelacionGastos");

        }

        // GET: Fondos/Create
        public async Task<IActionResult> Create()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var fondos = await _repoCuentas.ObtenerFondos(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(fondos, "Id", "Descricion");

            TempData.Keep();

            return View();
        }

        // POST: Fondos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFondo,IdCodCuenta,Porcentaje,FechaInicio,FechaFin")] Fondo fondo)
        {
            //if (ModelState.IsValid)
            //{
            // validar fechas coherentes
            var fechas = DateTime.Compare(fondo.FechaInicio, fondo.FechaFin);
            if (fechas > 0)
            {
                var error1 = new ErrorViewModel()
                {
                    RequestId = "La fecha de inicio no puede ser posterior a la fecha final!"
                };

                return View("Error", error1);
            }
            else if (fechas == 0)
            {
                var error2 = new ErrorViewModel()
                {
                    RequestId = "La fecha de inicio no puede ser igual a la fecha final!"
                };

                return View("Error", error2);
            }
            // 
            var result = await _repoCuentas.CrearFondo(fondo);

            return RedirectToAction("RelaciondeGastos", "RelacionGastos");
            //}
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", fondo.IdCodCuenta);
            //return View(fondo);
        }

        // GET: Fondos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Fondos == null)
            {
                return NotFound();
            }

            var fondo = await _context.Fondos.FindAsync(id);
            if (fondo == null)
            {
                return NotFound();
            }
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var fondos = await _repoCuentas.ObtenerFondos(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(fondos, "Id", "Descricion");

            TempData.Keep();

            return View(fondo);
        }

        // POST: Fondos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFondo,IdCodCuenta,Porcentaje,FechaInicio,FechaFin")] Fondo fondo)
        {
            if (id != fondo.IdFondo)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                // validar fechas coherentes
                var fechas = DateTime.Compare(fondo.FechaInicio, fondo.FechaFin);
                if (fechas > 0)
                {
                    var error1 = new ErrorViewModel()
                    {
                        RequestId = "La fecha de inicio no puede ser posterior a la fecha final!"
                    };

                    return View("Error", error1);
                }
                else if (fechas == 0)
                {
                    var error2 = new ErrorViewModel()
                    {
                        RequestId = "La fecha de inicio no puede ser igual a la fecha final!"
                    };

                    return View("Error", error2);
                }
                // 
                var idFondo = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == fondo.IdCodCuenta).FirstAsync();

                fondo.IdCodCuenta = idFondo.IdCodCuenta;

                _context.Update(fondo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FondoExists(fondo.IdFondo))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("RelaciondeGastos", "RelacionGastos"); 
            //}
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", fondo.IdCodCuenta);
            //return View(fondo);
        }

        // GET: Fondos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Fondos == null)
            {
                return NotFound();
            }

            var fondo = await _context.Fondos
                .Include(f => f.IdCodCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdFondo == id);
            if (fondo == null)
            {
                return NotFound();
            }

            return View(fondo);
        }

        // POST: Fondos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Fondos == null)
            {
                return Problem("Entity set 'NuevaAppContext.Fondos'  is null.");
            }
            var fondo = await _context.Fondos.FindAsync(id);
            if (fondo != null)
            {
                _context.Fondos.Remove(fondo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("RelaciondeGastos", "RelacionGastos");

        }

        private bool FondoExists(int id)
        {
            return (_context.Fondos?.Any(e => e.IdFondo == id)).GetValueOrDefault();
        }
    }
}
