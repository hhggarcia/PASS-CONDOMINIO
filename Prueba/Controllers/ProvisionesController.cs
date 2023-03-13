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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class ProvisionesController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly PruebaContext _context;

        public ProvisionesController(ICuentasContablesRepository repoCuentas,
            PruebaContext context)
        {
            _repoCuentas = repoCuentas;
            _context = context;
        }

        // GET: Provisiones
        public async Task<IActionResult> Index()
        {

            var pruebaContext = _context.Provisiones
                .Include(p => p.IdCodCuentaNavigation)
                .Include(p => p.IdCodGastoNavigation);

            return View(await pruebaContext.ToListAsync());
        }

        // GET: Provisiones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Provisiones == null)
            {
                return NotFound();
            }

            var provision = await _context.Provisiones
                .Include(p => p.IdCodCuentaNavigation)
                .Include(p => p.IdCodGastoNavigation)
                .FirstOrDefaultAsync(m => m.IdProvision == id);

            if (provision == null)
            {
                return NotFound();
            }

            return View(provision);
        }

        // GET: Provisiones/Create
        public async Task<IActionResult> Create()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            // traer subcuenta provision
            var provisiones = await _repoCuentas.ObtenerProvisiones(idCondominio);
            // traer subcuentas gastos
            var gastos = await _repoCuentas.ObtenerGastos(idCondominio);

            var condominioMonedas = from m in _context.MonedaConds
                                    join c in _context.Moneda
                                    on m.IdMoneda equals c.IdMoneda
                                    where m.IdCondominio == idCondominio
                                    select m;


            ViewData["IdMoneda"] = new SelectList(condominioMonedas, "Simbolo", "Simbolo");
            ViewData["IdCodCuenta"] = new SelectList(provisiones, "Id", "Descricion");
            ViewData["IdCodGasto"] = new SelectList(gastos, "Id", "Descricion");

            TempData.Keep();
            return View();
        }

        // POST: Provisiones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProvision,IdCodGasto,IdCodCuenta,Monto,FechaInicio,FechaFin,MontoRef,ValorDolar,SimboloMoneda,SimboloRef")] Provision provision)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                // validar fechas coherentes
                var fechas = DateTime.Compare(provision.FechaInicio, provision.FechaFin);
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
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var result = await _repoCuentas.CrearProvision(provision, idCondominio);

                if (result)
                {
                    return RedirectToAction("RelaciondeGastos", "RelacionGastos");
                }

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Ha ocurrido un error al registrar la Provisión!"
                };

                return View("Error", modeloError);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }


            //}
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", provision.IdCodCuenta);
            //ViewData["IdCodGasto"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", provision.IdCodGasto);
            //return View(provision);
        }

        // GET: Provisiones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Provisiones == null)
            {
                return NotFound();
            }

            var provision = await _context.Provisiones.FindAsync(id);
            if (provision == null)
            {
                return NotFound();
            }

            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            // traer subcuenta provision
            var provisiones = await _repoCuentas.ObtenerProvisiones(idCondominio);
            // traer subcuentas gastos
            var gastos = await _repoCuentas.ObtenerGastos(idCondominio);

            var condominioMonedas = from m in _context.MonedaConds
                                    join c in _context.Moneda
                                    on m.IdMoneda equals c.IdMoneda
                                    where m.IdCondominio == idCondominio
                                    select m;


            ViewData["IdMoneda"] = new SelectList(condominioMonedas, "Simbolo", "Simbolo");
            ViewData["IdCodCuenta"] = new SelectList(provisiones, "Id", "Descricion");
            ViewData["IdCodGasto"] = new SelectList(gastos, "Id", "Descricion");

            TempData.Keep();
            return View(provision);
        }

        // POST: Provisiones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProvision,IdCodGasto,IdCodCuenta,Monto,FechaInicio,FechaFin,MontoRef,ValorDolar,SimboloMoneda,SimboloRef")] Provision provision)
        {
            if (id != provision.IdProvision)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var fechas = DateTime.Compare(provision.FechaInicio, provision.FechaFin);
                if (fechas > 0)
                {
                    var error1 = new ErrorViewModel()
                    {
                        RequestId = "La fecha de incio no puede ser posterior a la fecha final!"
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

                var result = await _repoCuentas.EditarProvision(provision, idCondominio);

                TempData.Keep();

                return RedirectToAction("RelaciondeGastos", "RelacionGastos");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProvisionExists(provision.IdProvision))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            //return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", provision.IdCodCuenta);
            //ViewData["IdCodGasto"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", provision.IdCodGasto);
            //return View(provision);
        }

        // GET: Provisiones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Provisiones == null)
            {
                return NotFound();
            }

            var provision = await _context.Provisiones
                .Include(p => p.IdCodCuentaNavigation)
                .Include(p => p.IdCodGastoNavigation)
                .FirstOrDefaultAsync(m => m.IdProvision == id);
            if (provision == null)
            {
                return NotFound();
            }

            return View(provision);
        }

        // POST: Provisiones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Provisiones == null)
            {
                return Problem("Entity set 'PruebaContext.Provisiones'  is null.");
            }
            var provision = await _context.Provisiones.FindAsync(id);
            if (provision != null)
            {
                _context.Provisiones.Remove(provision);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("RelaciondeGastos", "RelacionGastos");

        }

        private bool ProvisionExists(int id)
        {
            return (_context.Provisiones?.Any(e => e.IdProvision == id)).GetValueOrDefault();
        }
    }
}
