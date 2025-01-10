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
    public class BonificacionsController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public BonificacionsController(ICuentasContablesRepository repoCuentas,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _repoCuentas = repoCuentas;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: Bonificacions
        public async Task<IActionResult> Index(int id)
        {
            var nuevaAppContext = _context.Bonificaciones
                .Include(b => b.IdCodCuentaNavigation)
                .Include(b => b.IdEmpleadoNavigation)
                .Where(c => c.IdEmpleado == id);

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
        public async Task<IActionResult> Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);            

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre");

            TempData.Keep();

            return View();
        }

        // POST: Bonificacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBonificacion,IdEmpleado,IdCodCuenta,Concepto,Monto,Activo")] Bonificacion bonificacion)
        {
            var idCuenta = _context.SubCuenta.Where(c => c.Id == bonificacion.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();

            bonificacion.IdCodCuenta = idCodCuenta;
           
            ModelState.Remove(nameof(bonificacion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(bonificacion.IdEmpleadoNavigation));
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {

                var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                if (monedaPrincipal != null)
                {
                    bonificacion.RefMonto = bonificacion.Monto / monedaPrincipal.ValorDolar;
                }
                _context.Add(bonificacion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Empleados");
            }


            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", bonificacion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", bonificacion.IdEmpleado);

            TempData.Keep();
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
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", bonificacion.IdEmpleado);

            TempData.Keep();
            return View(bonificacion);
        }

        // POST: Bonificacions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdBonificacion,IdEmpleado,IdCodCuenta,Concepto,Monto,Activo")] Bonificacion bonificacion)
        {
            if (id != bonificacion.IdBonificacion)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(bonificacion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(bonificacion.IdEmpleadoNavigation));
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                try
                {
                    var idCuenta = _context.SubCuenta.Where(c => c.Id == bonificacion.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                    var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
                    bonificacion.IdCodCuenta = idCodCuenta;


                    var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                    if (monedaPrincipal != null)
                    {
                        bonificacion.RefMonto = bonificacion.Monto / monedaPrincipal.ValorDolar;
                    }

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
                return RedirectToAction("Index", "Empleados");
            }

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", bonificacion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", bonificacion.IdEmpleado);

            TempData.Keep();
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
            return RedirectToAction("Index", "Empleados");
        }

        private bool BonificacionExists(int id)
        {
            return _context.Bonificaciones.Any(e => e.IdBonificacion == id);
        }
    }
}
