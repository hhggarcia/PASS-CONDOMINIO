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
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class ConciliacionsController : Controller
    {
        private readonly IReportesRepository _repoReportes;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly NuevaAppContext _context;

        public ConciliacionsController(IReportesRepository repoReportes,
            ICuentasContablesRepository repoCuentas,
            NuevaAppContext context)
        {
            _repoReportes = repoReportes;
            _repoCuentas = repoCuentas;
            _context = context;
        }

        // GET: Conciliacions
        public async Task<IActionResult> Index()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Conciliacions.Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdCondominioNavigation)
                .Where(c => c.IdCondominio == idCondominio);

            TempData.Keep();
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Conciliacions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conciliacion = await _context.Conciliacions
                .Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdConciliacion == id);
            if (conciliacion == null)
            {
                return NotFound();
            }

            return View(conciliacion);
        }

        // GET: Conciliacions/Create
        public async Task<IActionResult> Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", idCondominio);

            TempData.Keep();

            return View();
        }

        // POST: Conciliacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdConciliacion,IdCondominio,IdCodCuenta,FechaEmision,SaldoInicial,SaldoFinal,Actual,Activo,FechaInicio,FechaFin")] Conciliacion conciliacion)
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ModelState.Remove(nameof(conciliacion.IdCodCuentaNavigation));
            ModelState.Remove(nameof(conciliacion.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                var idCuenta = _context.SubCuenta.Where(c => c.Id == conciliacion.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
                conciliacion.IdCodCuenta = idCodCuenta;

                _context.Add(conciliacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", conciliacion.IdCodCuenta);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", conciliacion.IdCondominio);

            TempData.Keep();
            return View(conciliacion);
        }

        // GET: Conciliacions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conciliacion = await _context.Conciliacions.FindAsync(id);
            if (conciliacion == null)
            {
                return NotFound();
            }
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", conciliacion.IdCodCuenta);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", conciliacion.IdCondominio);
            return View(conciliacion);
        }

        // POST: Conciliacions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdConciliacion,IdCondominio,IdCodCuenta,FechaEmision,SaldoInicial,SaldoFinal,Actual,Activo,FechaInicio,FechaFin")] Conciliacion conciliacion)
        {
            if (id != conciliacion.IdConciliacion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conciliacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConciliacionExists(conciliacion.IdConciliacion))
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
            ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", conciliacion.IdCodCuenta);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", conciliacion.IdCondominio);
            return View(conciliacion);
        }

        // GET: Conciliacions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conciliacion = await _context.Conciliacions
                .Include(c => c.IdCodCuentaNavigation)
                .Include(c => c.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdConciliacion == id);
            if (conciliacion == null)
            {
                return NotFound();
            }

            return View(conciliacion);
        }

        // POST: Conciliacions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conciliacion = await _context.Conciliacions.FindAsync(id);
            if (conciliacion != null)
            {
                _context.Conciliacions.Remove(conciliacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConciliacionExists(int id)
        {
            return _context.Conciliacions.Any(e => e.IdConciliacion == id);
        }

        public async Task<IActionResult> Conciliacion()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var cajas = await _repoCuentas.ObtenerCaja(idCondominio);
            var bancos = await _repoCuentas.ObtenerBancos(idCondominio);
            var subcuentas = bancos.Concat(cajas).ToList();

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");

            TempData.Keep();
            return View(new ItemConciliacionVM());
        }

        public async Task<IActionResult> BuscarConciliacion(FiltroBancoVM filtro)
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var cajas = await _repoCuentas.ObtenerCaja(idCondominio);
            var bancos = await _repoCuentas.ObtenerBancos(idCondominio);
            var subcuentas = bancos.Concat(cajas).ToList();

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");

            var modelo = await _repoReportes.LoadConciliacionPagos(filtro);
            TempData.Keep();

            return View("Conciliacion", modelo);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ConfirmarConciliacion(ItemConciliacionVM modelo)
        {
            // calcular total ingresos
            var ingresos = modelo.PagosIds.Where(c => c.Text == "Ingreso").ToList();
            // calcular total egresos
            var egresos = modelo.PagosIds.Where(c => c.Text == "Egreso").ToList();

            foreach (var item in egresos)
            {
                if (item.Selected)
                {
                    var pago = await _context.PagoEmitidos.FindAsync(Convert.ToInt32(item.Value));
                    modelo.TotalEgreso += pago != null ? pago.Monto : 0;
                }                
            }

            foreach (var item in ingresos)
            {
                if (item.Selected)
                {
                    var pago = await _context.PagoRecibidos.FindAsync(Convert.ToInt32(item.Value));
                    modelo.TotalIngreso += pago != null ? pago.Monto : 0;
                }
            }
            // calcular saldo final
            modelo.SaldoFinal = modelo.TotalIngreso - modelo.TotalEgreso;
            // saldo inicial
            // si el saldo inicial es 0 -> permitir guardar un saldo inicial
            return View(modelo);
        }


        public IActionResult Conciliar(ItemConciliacionVM modelo)
        {
            // validar saldo de banco
            // tomar los ids seleccionados
            // buscar los pagos emitidos
            // budcar los pagos recibidos
            // desactivar los pagos
            // desactivar los objetos relacionados
            // registrar conciliacion
            return View();
        }
    }
}
