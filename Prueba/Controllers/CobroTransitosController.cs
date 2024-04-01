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

    public class CobroTransitosController : Controller
    {
        private readonly IPagosRecibidosRepository _repoPagosRecibidos;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public CobroTransitosController(IPagosRecibidosRepository repoPagosRecibidos,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _repoPagosRecibidos = repoPagosRecibidos;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: CobroTransitos
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.CobroTransitos.Include(c => c.IdCondominioNavigation).Where(c => c.IdCondominio == IdCondominio);

            TempData.Keep();
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CobroTransitos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobroTransito = await _context.CobroTransitos
                .Include(c => c.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdCobroTransito == id);
            if (cobroTransito == null)
            {
                return NotFound();
            }

            return View(cobroTransito);
        }

        // GET: CobroTransitos/Create
        public IActionResult Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == IdCondominio), "IdCondominio", "Nombre");
            TempData.Keep();

            return View();
        }

        // POST: CobroTransitos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCobroTransito,IdCondominio,FormaPago,Monto,Fecha,Concepto,Factura,Recibo")] CobroTransito cobroTransito)
        {
            ModelState.Remove(nameof(cobroTransito.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(cobroTransito.IdCondominio)).First();

                cobroTransito.MontoRef = cobroTransito.Monto / monedaPrincipal.ValorDolar;
                _context.Add(cobroTransito);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cobroTransito.IdCondominio);
            return View(cobroTransito);
        }

        // GET: CobroTransitos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobroTransito = await _context.CobroTransitos.FindAsync(id);
            if (cobroTransito == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cobroTransito.IdCondominio);
            return View(cobroTransito);
        }

        // POST: CobroTransitos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCobroTransito,IdCondominio,FormaPago,Monto,Fecha,Concepto,Factura,Recibo")] CobroTransito cobroTransito)
        {
            if (id != cobroTransito.IdCobroTransito)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(cobroTransito.IdCondominioNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cobroTransito);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CobroTransitoExists(cobroTransito.IdCobroTransito))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cobroTransito.IdCondominio);
            return View(cobroTransito);
        }

        // GET: CobroTransitos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobroTransito = await _context.CobroTransitos
                .Include(c => c.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdCobroTransito == id);
            if (cobroTransito == null)
            {
                return NotFound();
            }

            return View(cobroTransito);
        }

        // POST: CobroTransitos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cobroTransito = await _context.CobroTransitos.FindAsync(id);
            if (cobroTransito != null)
            {
                _context.CobroTransitos.Remove(cobroTransito);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CobroTransitoExists(int id)
        {
            return _context.CobroTransitos.Any(e => e.IdCobroTransito == id);
        }

        public async Task<IActionResult> CobroTransito()
        {
            try
            {
                //traer subcuentas del condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoPagosRecibidos.FormCobroTransito(idCondominio);

                TempData.Keep();

                return View(modelo);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CobroTransito(CobroTransitoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {
                    modelo.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    if (modelo.Pagoforma == FormaPago.Transferencia)
                    {
                        var existPagoTransferencia = from pago in _context.PagoRecibidos
                                                     join referencia in _context.ReferenciasPrs
                                                     on pago.IdPagoRecibido equals referencia.IdPagoRecibido
                                                     where referencia.NumReferencia == modelo.NumReferencia
                                                     select new { pago, referencia };

                        if (existPagoTransferencia != null && existPagoTransferencia.Any())
                        {
                            //var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                            modelo = await _repoPagosRecibidos.FormCobroTransito(modelo.IdCondominio);

                            TempData.Keep();

                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe una transferencia con este número de referencia!";

                            return View("CobroTransito", modelo);
                        }
                    }

                    var resultado = await _repoPagosRecibidos.RegistrarCobroTransito(modelo);

                    if (resultado == "exito")
                    {
                        TempData.Keep();

                        return RedirectToAction("Index");
                    }

                    modelo = await _repoPagosRecibidos.FormCobroTransito(modelo.IdCondominio);

                    TempData.Keep();

                    return View("CobroTransito", modelo);
                }

                modelo = await _repoPagosRecibidos.FormCobroTransito(modelo.IdCondominio);

                TempData.Keep();

                return View("CobroTransito", modelo);
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
    }
}
