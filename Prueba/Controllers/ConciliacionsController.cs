using System;
using System.Collections.Generic;
using System.Globalization;
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
            TempData["FechaInicio"] = filtro.FechaInicio.ToString("dd-MM-yyyy");
            TempData["FechaFin"] = filtro.FechaFin.ToString("dd-MM-yyyy");

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
            TempData.Keep();
            return View(modelo);
        }


        public async Task<IActionResult> Conciliar(ItemConciliacionVM modelo)
        {
            try
            {
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                string fechaEnCadenaInicio = TempData["FechaInicio"].ToString();
                DateTime fechaInicio = DateTime.ParseExact(fechaEnCadenaInicio, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                string fechaEnCadenaFin = TempData["FechaFin"].ToString();
                DateTime fechaFin = DateTime.ParseExact(fechaEnCadenaFin, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                // validar saldo de banco
                // calcular total ingresos
                var ingresos = modelo.PagosIds.Where(c => c.Text == "Ingreso").ToList();
                // calcular total egresos
                var egresos = modelo.PagosIds.Where(c => c.Text == "Egreso").ToList();

                // registrar conciliacion
                var conciliacion = new Conciliacion
                {
                    IdCondominio = idCondominio,
                    IdCodCuenta = modelo.IdCodigoCuenta,
                    FechaEmision = DateTime.Today,
                    SaldoFinal = modelo.SaldoFinal,
                    SaldoInicial = modelo.SaldoInicial,
                    Actual = true,
                    Activo = true,
                    TotalEgreso = modelo.TotalEgreso,
                    TotalIngreso = modelo.TotalIngreso,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                _context.Conciliacions.Add(conciliacion);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    // tomar los ids seleccionados
                    // buscar los pagos emitidos
                    // budcar los pagos recibidos
                    foreach (var item in egresos)
                    {
                        if (item.Selected)
                        {
                            var pago = await _context.PagoEmitidos.FindAsync(Convert.ToInt32(item.Value));
                            // desactivar los pagos
                            // desactivar los objetos relacionados
                            if (pago != null)
                            {
                                var anticiposNomina = await _context.AnticipoNominas.Where(c => c.IdPagoEmitido == pago.IdPagoEmitido).ToListAsync();
                                var ordenPago = await _context.OrdenPagos.Where(c => c.IdPagoEmitido == pago.IdPagoEmitido).ToListAsync();
                                var pagoAnticipo = await _context.PagoAnticipos.Where(c => c.IdPagoEmitido == pago.IdPagoEmitido).ToListAsync();
                                var pagoFactura = await _context.PagoFacturas.Where(c => c.IdPagoEmitido == pago.IdPagoEmitido).ToListAsync();
                                var pagoNomina = await _context.PagosNominas.Where(c => c.IdPagoEmitido == pago.IdPagoEmitido).ToListAsync();

                                if (anticiposNomina != null)
                                {
                                    foreach (var relacion in anticiposNomina)
                                    {
                                        var anticipo = await _context.AnticipoNominas.FindAsync(relacion.IdAnticipoNomina);

                                        if (anticipo != null)
                                        {
                                            anticipo.Activo = false;
                                            _context.AnticipoNominas.Update(anticipo);
                                        }
                                    }
                                }

                                if (ordenPago != null)
                                {
                                    foreach (var relacion in ordenPago)
                                    {
                                        var ordenpago = await _context.OrdenPagos.FindAsync(relacion.IdOrdenPago);

                                        if (ordenpago != null)
                                        {
                                            ordenpago.Activo = false;
                                            _context.OrdenPagos.Update(ordenpago);
                                        }
                                    }
                                }

                                if (pagoAnticipo != null)
                                {
                                    foreach (var relacion in pagoAnticipo)
                                    {
                                        var anticipo = await _context.Anticipos.FindAsync(relacion.IdAnticipo);

                                        if (anticipo != null)
                                        {
                                            anticipo.Activo = false;
                                            _context.Anticipos.Update(anticipo);
                                        }
                                    }
                                }

                                if (pagoFactura != null)
                                {
                                    foreach (var relacion in pagoFactura)
                                    {
                                        var factura = await _context.Facturas.FindAsync(relacion.IdFactura);

                                        if (factura != null)
                                        {
                                            factura.Activo = false;
                                            _context.Facturas.Update(factura);
                                        }
                                    }
                                }

                                if (pagoNomina != null)
                                {
                                    foreach (var relacion in pagoNomina)
                                    {
                                        var recibo = await _context.ReciboNominas.FindAsync(relacion.IdReciboNomina);

                                        if (recibo != null)
                                        {
                                            recibo.Activo = false;
                                            _context.ReciboNominas.Update(recibo);
                                        }
                                    }
                                }

                                var pagoConciliacion = new ConciliacionPagoEmitido
                                {
                                    IdConciliacion = conciliacion.IdConciliacion,
                                    IdPagoEmitido = pago.IdPagoEmitido
                                };

                                pago.Activo = false;

                                _context.PagoEmitidos.Update(pago);
                                _context.ConciliacionPagoEmitidos.Add(pagoConciliacion);
                            }
                        }
                    }

                    foreach (var item in ingresos)
                    {
                        if (item.Selected)
                        {
                            var pago = await _context.PagoRecibidos.FindAsync(Convert.ToInt32(item.Value));

                            if (pago != null)
                            {
                                // desactivar los pagos
                                // desactivar los objetos relacionados
                                var cobroTransito = await _context.PagoCobroTransitos.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                                var pagoFacturaEmitida = await _context.PagoFacturaEmitida.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                                var pagoPropiedad = await _context.PagoPropiedads.Where(c => c.IdPago == pago.IdPagoRecibido).ToListAsync();
                                var pagoReserva = await _context.PagoReservas.Where(c => c.IdPago == pago.IdPagoRecibido).ToListAsync();
                                var pagoCuotas = await _context.PagosCuotas.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                                //var pagoRecibos = await _context.PagosRecibos.Where(c => c.IdPago == pago.IdPagoRecibido).ToListAsync();

                                if (cobroTransito != null)
                                {
                                    foreach (var relacion in cobroTransito)
                                    {
                                        var cobro = await _context.CobroTransitos.FindAsync(relacion.IdCobroTransito);

                                        if (cobro != null)
                                        {
                                            cobro.Activo = false;
                                            _context.CobroTransitos.Update(cobro);
                                        }
                                    }
                                }

                                if (pagoFacturaEmitida != null)
                                {
                                    foreach (var relacion in pagoFacturaEmitida)
                                    {
                                        var factura = await _context.FacturaEmitida.FindAsync(relacion.IdFactura);

                                        if (factura != null)
                                        {
                                            factura.Activo = false;
                                            _context.FacturaEmitida.Update(factura);
                                        }
                                    }
                                }

                                if (pagoPropiedad != null)
                                {
                                    foreach (var relacion in pagoPropiedad)
                                    {
                                        var pagoProp = await _context.PagoPropiedads.FindAsync(relacion.IdPagoPropiedad);

                                        if (pagoProp != null)
                                        {
                                            pagoProp.Activo = false;
                                            _context.PagoPropiedads.Update(pagoProp);
                                        }
                                    }
                                }

                                if (pagoReserva != null)
                                {
                                    foreach (var relacion in pagoReserva)
                                    {
                                        var reserva = await _context.ReciboReservas.FindAsync(relacion.IdReciboReserva);

                                        if (reserva != null)
                                        {
                                            reserva.Activo = false;
                                            _context.ReciboReservas.Update(reserva);
                                        }
                                    }
                                }

                                if (pagoCuotas != null)
                                {
                                    foreach (var relacion in pagoCuotas)
                                    {
                                        var cuota = await _context.ReciboCuotas.FindAsync(relacion.IdReciboCuota);

                                        if (cuota != null)
                                        {
                                            cuota.Activo = false;
                                            _context.ReciboCuotas.Update(cuota);
                                        }
                                    }
                                }

                                var pagoConciliacion = new ConciliacionPagoRecibido
                                {
                                    IdConciliacion = conciliacion.IdConciliacion,
                                    IdPagoRecibido = pago.IdPagoRecibido
                                };

                                pago.Activo = false;

                                _context.PagoRecibidos.Update(pago);
                                _context.ConciliacionPagoRecibidos.Add(pagoConciliacion);
                            }
                        }
                    }

                    TempData.Keep();
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Error al crear la conciliación"
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
        }
    }
}
