using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Wordprocessing;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class MonedaCuentasController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly PruebaContext _context;

        public MonedaCuentasController(ICuentasContablesRepository repoCuentas,
            PruebaContext context)
        {
            _repoCuentas = repoCuentas;
            _context = context;
        }

        // GET: MonedaCuentas
        public async Task<IActionResult> Index()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var pruebaContext = _context.MonedaCuenta.Include(m => m.IdCodCuentaNavigation).Include(m => m.IdMonedaNavigation);
            var codigos = await _repoCuentas.ObtenerCuentasCond(idCondominio);
            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            var modelo = new MonedaCuentasVM()
            {
                MonedaCuentas = await pruebaContext.ToListAsync(),
                SubCuentas = subcuentas,
                Codigos = codigos
            };

            TempData.Keep();

            return View(modelo);
        }

        // GET: MonedaCuentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MonedaCuenta == null)
            {
                return NotFound();
            }

            var monedaCuenta = await _context.MonedaCuenta
                .Include(m => m.IdCodCuentaNavigation)
                .Include(m => m.IdMonedaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monedaCuenta == null)
            {
                return NotFound();
            }

            return View(monedaCuenta);
        }

        // GET: MonedaCuentas/Create
        public async Task<IActionResult> Create()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);
            var monedas = await _context.MonedaConds.Where(c => c.IdCondominio == idCondominio).ToListAsync();

            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");
            ViewData["IdMoneda"] = new SelectList(monedas, "IdMonedaCond", "Simbolo");

            TempData.Keep();

            return View();
        }

        // POST: MonedaCuentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCodCuenta,IdMoneda,RecibePagos,SaldoInicial,SaldoFinal")] MonedaCuenta monedaCuenta)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                // idCodCuenta es ID de Subcuenta
                // buscar por Codigo en CodigoCuentasGlobal
                var cc = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == monedaCuenta.IdCodCuenta).ToListAsync();

                // validar no repetido
                var exist = await _context.MonedaCuenta.Where(c => c.IdCodCuenta == cc.First().IdCodCuenta && c.IdMoneda == monedaCuenta.IdMoneda).ToListAsync();

                if (exist.Any())
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una moneda asignada a esta Cuenta!"
                    };

                    return View("Error", modeloError);
                }
                // cambiar idCodCuenta
                monedaCuenta.IdCodCuenta = cc.First().IdCodCuenta;

                _context.Add(monedaCuenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", monedaCuenta.IdCodCuenta);
            //ViewData["IdMoneda"] = new SelectList(_context.MonedaConds, "IdMonedaCond", "IdMonedaCond", monedaCuenta.IdMoneda);
            //return View(monedaCuenta);
        }

        // GET: MonedaCuentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || _context.MonedaCuenta == null)
                {
                    return NotFound();
                }

                var monedaCuenta = await _context.MonedaCuenta.FindAsync(id);
                if (monedaCuenta == null)
                {
                    return NotFound();
                }
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);
                var monedas = await _context.MonedaConds.Where(c => c.IdCondominio == idCondominio).ToListAsync();
                var cc = await _context.CodigoCuentasGlobals.FindAsync(monedaCuenta.IdCodCuenta);

                ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", cc.IdCodigo);
                ViewData["IdMoneda"] = new SelectList(monedas, "IdMonedaCond", "Simbolo", monedaCuenta.IdMoneda);

                return View(monedaCuenta);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }

        // POST: MonedaCuentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCodCuenta,IdMoneda,RecibePagos,SaldoInicial,SaldoFinal")] MonedaCuenta monedaCuenta)
        {
            if (id != monedaCuenta.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                // idCodCuenta es ID de Subcuenta
                // buscar por Codigo en CodigoCuentasGlobal
                var cc = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == monedaCuenta.IdCodCuenta).ToListAsync();
                // validar no repetido
                var exist = await _context.MonedaCuenta.Where(c => c.IdCodCuenta == cc.First().IdCodCuenta
                                                                    && cc.First().IdCodCuenta != monedaCuenta.IdCodCuenta
                                                                    && c.IdMoneda == monedaCuenta.IdMoneda)
                                                       .ToListAsync();

                if (exist.Any())
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una moneda asignada a esta Cuenta!"
                    };

                    return View("Error", modeloError);
                }
                // cambiar idCodCuenta
                monedaCuenta.IdCodCuenta = cc.First().IdCodCuenta;
                _context.Update(monedaCuenta);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

            return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdCodCuenta"] = new SelectList(_context.CodigoCuentasGlobals, "IdCodCuenta", "IdCodCuenta", monedaCuenta.IdCodCuenta);
            //ViewData["IdMoneda"] = new SelectList(_context.MonedaConds, "IdMonedaCond", "IdMonedaCond", monedaCuenta.IdMoneda);
            //return View(monedaCuenta);
        }

        // GET: MonedaCuentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MonedaCuenta == null)
            {
                return NotFound();
            }

            var monedaCuenta = await _context.MonedaCuenta
                .Include(m => m.IdCodCuentaNavigation)
                .Include(m => m.IdMonedaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monedaCuenta == null)
            {
                return NotFound();
            }

            return View(monedaCuenta);
        }

        // POST: MonedaCuentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.MonedaCuenta == null)
                {
                    return Problem("Entity set 'PruebaContext.MonedaCuenta'  is null.");
                }
                var monedaCuenta = await _context.MonedaCuenta.FindAsync(id);
                if (monedaCuenta != null)
                {
                    _context.MonedaCuenta.Remove(monedaCuenta);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
            return RedirectToAction(nameof(Index));

        }

        private bool MonedaCuentaExists(int id)
        {
            return (_context.MonedaCuenta?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
