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

    public class DeduccionesController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public DeduccionesController(ICuentasContablesRepository repoCuentas,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _repoCuentas = repoCuentas;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: Deducciones
        //public async Task<IActionResult> Index()
        //{
        //    var nuevaAppContext = _context.Deducciones.Include(d => d.IdEmpleadoNavigation);
        //    return View(await nuevaAppContext.ToListAsync());
        //}

        // GET: Deducciones
        public async Task<IActionResult> Index(int id)
        {
            var nuevaAppContext = _context.Deducciones
                .Include(b => b.IdCodCuentaNavigation)
                .Include(d => d.IdEmpleadoNavigation)
                .Where(c => c.IdEmpleado == id);

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
                .Include(b => b.IdCodCuentaNavigation)
                .Include(d => d.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdDeduccion == id);
            if (deduccion == null)
            {
                return NotFound();
            }

            return View(deduccion);
        }

        // GET: Deducciones/Create
        public async Task<IActionResult> Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");     
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre");

            TempData.Keep();

            return View();
        }

        // POST: Deducciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDeduccion,Concepto,Monto,Activo,IdEmpleado,IdCodCuenta")] Deduccion deduccion)
        {
            ModelState.Remove(nameof(deduccion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(deduccion.IdEmpleadoNavigation));
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                var idCuenta = _context.SubCuenta.Where(c => c.Id == deduccion.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();

                deduccion.IdCodCuenta = idCodCuenta;


                var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                if (monedaPrincipal != null)
                {
                    deduccion.RefMonto = deduccion.Monto / monedaPrincipal.ValorDolar;
                }
                _context.Add(deduccion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Empleados");

            }


            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", deduccion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", deduccion.IdEmpleado);

            TempData.Keep();
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
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", deduccion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", deduccion.IdEmpleado);

            TempData.Keep();
            return View(deduccion);
        }

        // POST: Deducciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDeduccion,Concepto,Monto,Activo,IdEmpleado,IdCodCuenta")] Deduccion deduccion)
        {
            if (id != deduccion.IdDeduccion)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(deduccion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(deduccion.IdEmpleadoNavigation));
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                try
                {
                    var idCuenta = _context.SubCuenta.Where(c => c.Id == deduccion.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                    var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();

                    deduccion.IdCodCuenta = idCodCuenta;


                    var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                    if (monedaPrincipal != null)
                    {
                        deduccion.RefMonto = deduccion.Monto / monedaPrincipal.ValorDolar;
                    }
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

                return RedirectToAction("Index", "Empleados");
            }

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", deduccion.IdCodCuenta);
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", deduccion.IdEmpleado);

            TempData.Keep();
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
            return RedirectToAction("Index", "Empleados");
        }

        private bool DeduccionExists(int id)
        {
            return _context.Deducciones.Any(e => e.IdDeduccion == id);
        }
    }
}
