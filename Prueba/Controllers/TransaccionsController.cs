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

    public class TransaccionsController : Controller
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly NuevaAppContext _context;

        public TransaccionsController(IMonedaRepository repoMoneda,
            ICuentasContablesRepository repoCuentas,
            NuevaAppContext context)
        {
            _repoMoneda = repoMoneda;
            _repoCuentas = repoCuentas;
            _context = context;
        }

        // GET: Transaccions
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Transaccions
                .Include(t => t.IdCodCuentaNavigation)
                .Include(t => t.IdPropiedadNavigation)
                .Include(t => t.IdProveedorNavigation);

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Transaccions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaccion = await _context.Transaccions
                .Include(t => t.IdCodCuentaNavigation)
                .Include(t => t.IdPropiedadNavigation)
                .Include(t => t.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdTransaccion == id);
            if (transaccion == null)
            {
                return NotFound();
            }

            return View(transaccion);
        }

        // GET: Transaccions/Create
        public async Task<IActionResult> Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre");

            TempData.Keep();
            return View();
        }

        // POST: Transaccions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTransaccion,TipoTransaccion,IdPropiedad,Fecha,IdCodCuenta,Descripcion,Documento,MontoTotal,Cancelado,Activo")] Transaccion transaccion, bool check, bool checkActivo)
        {
            ModelState.Remove(nameof(transaccion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(transaccion.IdPropiedadNavigation));
            ModelState.Remove(nameof(transaccion.IdProveedorNavigation));

            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                var idCuenta = _context.SubCuenta
                    .Where(c => c.Id == transaccion.IdCodCuenta)
                    .Select(c => c.Id)
                    .FirstOrDefault();

                var idCodCuenta = _context.CodigoCuentasGlobals
                    .Where(c => c.IdSubCuenta == idCuenta)
                    .Select(c => c.IdCodCuenta)
                    .FirstOrDefault();
                // buscar grupo de la cuenta
                var grupo = await (from g in _context.GrupoGastos
                                   join cg in _context.CuentasGrupos
                                   on g.IdGrupoGasto equals cg.IdGrupoGasto
                                   where idCodCuenta == cg.IdCodCuenta
                                   select g).FirstOrDefaultAsync();

                transaccion.IdCodCuenta = idCodCuenta;
                transaccion.IdGrupo = grupo != null ? grupo.IdGrupoGasto : 0;
                transaccion.Activo = checkActivo;


                var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                if (monedaPrincipal != null)
                {
                    transaccion.MontoRef = transaccion.MontoTotal / monedaPrincipal.ValorDolar;
                    transaccion.ValorDolar = monedaPrincipal.ValorDolar;
                    transaccion.SimboloMoneda = "Bs";
                    transaccion.SimboloRef = "$";
                }

                if (check)
                {
                    transaccion.IdPropiedad = null;
                }

                _context.Add(transaccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", transaccion.IdCodCuenta);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", transaccion.IdPropiedad);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", transaccion.IdProveedor);

            TempData.Keep();

            return View(transaccion);
        }

        // GET: Transaccions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaccion = await _context.Transaccions.FindAsync(id);
            if (transaccion == null)
            {
                return NotFound();
            }
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", transaccion.IdCodCuenta);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", transaccion.IdPropiedad);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", transaccion.IdProveedor);

            TempData.Keep();
            return View(transaccion);
        }

        // POST: Transaccions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTransaccion,TipoTransaccion,IdPropiedad,IdCodCuenta,Fecha,Descripcion,IdProveedor,Documento,MontoTotal,Cancelado,Activo")] Transaccion transaccion, bool check, bool checkActivo)
        {
            if (id != transaccion.IdTransaccion)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(transaccion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(transaccion.IdPropiedadNavigation));
            ModelState.Remove(nameof(transaccion.IdProveedorNavigation));
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                try
                {
                    var idCuenta = _context.SubCuenta.Where(c => c.Id == transaccion.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                    var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
                    // buscar grupo de la cuenta
                    var grupo = await (from g in _context.GrupoGastos
                                       join cg in _context.CuentasGrupos
                                       on g.IdGrupoGasto equals cg.IdGrupoGasto
                                       where idCodCuenta == cg.IdCodCuenta
                                       select g).FirstOrDefaultAsync();

                    transaccion.IdCodCuenta = idCodCuenta;
                    transaccion.IdGrupo = grupo != null ? grupo.IdGrupoGasto : 0;
                    transaccion.Activo = checkActivo;


                    var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                    if (monedaPrincipal != null)
                    {
                        transaccion.MontoRef = transaccion.MontoTotal / monedaPrincipal.ValorDolar;
                        transaccion.ValorDolar = monedaPrincipal.ValorDolar;
                        transaccion.SimboloMoneda = "Bs";
                        transaccion.SimboloRef = "$";
                    }

                    if (check)
                    {
                        transaccion.IdPropiedad = null;
                    }

                    _context.Update(transaccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransaccionExists(transaccion.IdTransaccion))
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

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", transaccion.IdCodCuenta);
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", transaccion.IdPropiedad);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", transaccion.IdProveedor);

            TempData.Keep();
            return View(transaccion);
        }

        // GET: Transaccions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaccion = await _context.Transaccions
                .Include(t => t.IdCodCuentaNavigation)
                .Include(t => t.IdPropiedadNavigation)
                .Include(t => t.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdTransaccion == id);
            if (transaccion == null)
            {
                return NotFound();
            }

            return View(transaccion);
        }

        // POST: Transaccions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaccion = await _context.Transaccions.FindAsync(id);
            if (transaccion != null)
            {
                _context.Transaccions.Remove(transaccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransaccionExists(int id)
        {
            return _context.Transaccions.Any(e => e.IdTransaccion == id);
        }
    }
}
