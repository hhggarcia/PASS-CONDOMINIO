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
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class CobroTransitosController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly IPagosRecibidosRepository _repoPagosRecibidos;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public CobroTransitosController(IPDFServices servicesPDF,
            IPagosRecibidosRepository repoPagosRecibidos,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
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
                cobroTransito.Activo = true;
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
                var pagosCobros = _context.PagoCobroTransitos.Where(c => c.IdCobroTransito == id).ToList();

                if (pagosCobros.Any())
                {
                    foreach (var item in pagosCobros)
                    {
                        var pago = await _context.PagoRecibidos.FindAsync(item.IdPagoRecibido);

                        if (pago != null)
                        {
                            var referencia = await _context.ReferenciasPrs.FirstOrDefaultAsync(c => c.IdPagoRecibido == pago.IdPagoRecibido);
                            if (referencia != null)
                            {
                                _context.ReferenciasPrs.Remove(referencia);
                            }
                             
                            _context.PagoCobroTransitos.Remove(item);
                            _context.PagoRecibidos.Remove(pago);
                        }
                    }
                    //_context.PagoCobroTransitos.RemoveRange(pagosCobros);

                }
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

        public async Task<IActionResult> CobroTransitoPDF(int id)
        {
            var cobroTransito = await _context.CobroTransitos.FindAsync(id);

            if (cobroTransito != null)
            {
                var data = await _servicesPDF.ComprobanteCobroTransitoPDF(cobroTransito);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "ComprobanteCobroTransito.pdf");
            }

            return RedirectToAction("Index");
        }

        public IActionResult AsignarFactura()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCobros"] = new SelectList(_context.CobroTransitos
                .Where(c => c.IdCondominio == IdCondominio && !c.Factura && c.Activo), "IdCobroTransito", "Concepto");

            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida
                .Include(c => c.IdClienteNavigation)
                .Where(c => c.IdClienteNavigation.IdCondominio == IdCondominio && 
                !c.Pagada && 
                !c.Anulada && 
                c.Activo), "IdFacturaEmitida", "NumFactura");

            TempData.Keep();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsginarFactura(AsignarCobroTransitoFacturaVM modelo)
        {
            // buscar cobro
            var cobro = await _context.CobroTransitos.FindAsync(modelo.IdCobroTransito);
            // buscar factura
            var factura = await _context.FacturaEmitida.FindAsync(modelo.IdFactura);
            // buscar pago recibido
            var relacion = await _context.PagoCobroTransitos
                .FirstOrDefaultAsync(c => c.IdCobroTransito == modelo.IdCobroTransito);

            if (modelo.RetIslr && (modelo.NumComprobanteRetIslr == "" || modelo.NumComprobanteRetIslr == null))
            {
                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "El numero de Comprobante ISLR no puede estar vacio!";

                return View(modelo);
            }

            if (modelo.RetIva && (modelo.NumComprobanteRetIva == "" || modelo.NumComprobanteRetIva == null))
            {
                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "El numero de Comprobante IVA no puede estar vacio!";

                return View(modelo);
            }

            if (cobro != null && factura != null && relacion != null)
            {               

                var pago = await _context.PagoRecibidos.FindAsync(relacion.IdPagoRecibido);

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.IdCliente == factura.IdCliente);

                var itemLibroVenta = await _context.LibroVentas
                    .Where(c => c.IdFactura == factura.IdFacturaEmitida)
                    .FirstOrDefaultAsync();

                var itemCuentaCobrar = await _context.CuentasCobrars
                    .Where(c => c.IdFactura == factura.IdFacturaEmitida)
                    .FirstOrDefaultAsync();

                if (pago != null && cliente != null && itemCuentaCobrar != null && itemLibroVenta != null)
                {
                    
                    // registrar pago-factura
                    PagoFacturaEmitida pagoFactura = new PagoFacturaEmitida
                    {
                        IdPagoRecibido = pago.IdPagoRecibido,
                        IdFactura = modelo.IdFactura
                    };

                    var montoPagar = factura.MontoTotal - (modelo.RetIva ? itemLibroVenta.RetIva : 0) - (modelo.RetIslr ? itemLibroVenta.RetIslr : 0);
                    // evaluar si queda abonada o pagada
                    if (factura.Abonado == 0)
                    {
                        if (pago.Monto < montoPagar)
                        {
                            factura.Abonado += pago.Monto;
                            cliente.Deuda -= pago.Monto;
                        }
                        else if (pago.Monto == montoPagar)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
                            cliente.Deuda -= pago.Monto;

                        } else
                        {
                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "El monto es mayor al total de la Factura!";

                            return View(modelo);
                        }

                    }
                    else
                    {
                        if ((pago.Monto + factura.Abonado) < montoPagar)
                        {
                            factura.Abonado += pago.Monto;
                            cliente.Deuda -= pago.Monto;
                        }
                        else if ((pago.Monto + factura.Abonado) == montoPagar)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
                            cliente.Deuda -= pago.Monto;
                        }
                        else
                        {
                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "El monto más lo abonado en la factura excede el total de la Factura!";

                            return View(modelo);
                        }
                    }
                    
                    // registrar comprobantes de IVA e ISLR
                    if (modelo.RetIva)
                    {
                        var existRet = await _context.CompRetIvaClientes
                            .Where(c => c.NumCompRet == modelo.NumComprobanteRetIva)
                            .ToListAsync();

                        if (!existRet.Any())
                        {
                            var retIva = new CompRetIvaCliente
                            {
                                IdFactura = modelo.IdFactura,
                                IdCliente = cliente.IdCliente,
                                FechaEmision = modelo.FechaEmisionRetIva,
                                TipoTransaccion = true,
                                NumFacturaAfectada = factura.NumFactura.ToString(),
                                TotalCompraIva = factura.MontoTotal,
                                CompraSinCreditoIva = 0,
                                BaseImponible = itemLibroVenta.BaseImponible,
                                Alicuota = 16,
                                ImpIva = itemLibroVenta.Iva,
                                IvaRetenido = itemLibroVenta.RetIva,
                                TotalCompraRetIva = factura.MontoTotal - itemLibroVenta.RetIva,
                                NumCompRet = modelo.NumComprobanteRetIva,
                                NumComprobante = 1
                            };

                            itemLibroVenta.ComprobanteRetencion = modelo.NumComprobanteRetIva;
                            itemLibroVenta.IvaRetenido = itemLibroVenta.RetIva;

                            _context.LibroVentas.Update(itemLibroVenta);
                            _context.CompRetIvaClientes.Add(retIva);
                        }
                        else
                        {
                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe un comprobante de IVA con el numero "+ modelo.NumComprobanteRetIva +"!";

                            return View(modelo);
                        }
                    }

                    if (modelo.RetIslr)
                    {
                        var ret = (from c in _context.Clientes
                                   join v in _context.Islrs
                                   on c.IdRetencionIslr equals v.Id
                                   where c.IdCliente == cliente.IdCliente
                                   select v).FirstOrDefault();

                        var existRet = await _context.ComprobanteRetencionClientes
                            .Where(c => c.NumCompRet == modelo.NumComprobanteRetIslr)
                            .ToListAsync();

                        if (ret != null && existRet.Count == 0)
                        {
                            var retIslr = new ComprobanteRetencionCliente
                            {
                                IdCliente = cliente.IdCliente,
                                IdFactura = modelo.IdFactura,
                                FechaEmision = modelo.FechaEmisionIslr,
                                Description = ret.Concepto,
                                Retencion = ret.Tarifa,
                                Sustraendo = ret.Sustraendo,
                                ValorRetencion = itemLibroVenta.RetIslr,
                                TotalImpuesto = itemLibroVenta.RetIslr,
                                NumCompRet = modelo.NumComprobanteRetIslr,
                                NumComprobante = 1,
                                TotalFactura = factura.MontoTotal,
                                BaseImponible = itemLibroVenta.BaseImponible
                            };

                            _context.ComprobanteRetencionClientes.Add(retIslr);

                        }
                        else
                        {
                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe un comprobante de IVA con el numero " + modelo.NumComprobanteRetIslr + "!";

                            return View(modelo);
                        }
                    }

                    // actualizar cobro
                    cobro.Activo = false;
                    cobro.Asignado = true;
                    cobro.Factura = true;
                    cobro.IdFactura = modelo.IdFactura;

                    _context.PagoFacturaEmitida.Add(pagoFactura);
                    _context.CobroTransitos.Update(cobro);
                    _context.FacturaEmitida.Update(factura);

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");

                }

            }

            return View(modelo);
        }

        public IActionResult AsignarRecibo()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCobros"] = new SelectList(_context.CobroTransitos
                .Where(c => c.IdCondominio == IdCondominio && !c.Factura && c.Activo), "IdCobroTransito", "Concepto");

            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads
                .Include(c => c.IdCondominioNavigation)
                .Where(c => c.IdCondominioNavigation.IdCondominio == IdCondominio)
                .OrderBy(c => c.Codigo)
                .ToList(), "IdPropiedad", "Codigo");

            TempData.Keep();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRecibo(AsignarCobroTransitoReciboVM modelo)
        {
            try
            {
                // buscar propiedad
                var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
                var cobro = await _context.CobroTransitos.FindAsync(modelo.IdPropiedad);
                // buscar recibos seleccionados
                if (modelo.ListRecibos != null && modelo.ListRecibos.Any())
                {
                    foreach (var item in modelo.ListRecibos)
                    {
                        if (item.Selected)
                        {
                            var idRecibo = Convert.ToInt32(item.Value);
                            var recibo = await _context.ReciboCobros.FindAsync(idRecibo);
                            if (recibo != null)
                            {
                                modelo.Recibos.Add(recibo);
                            }
                        }
                    }
                }
                if (propiedad != null && cobro != null)
                {
                    var pago = await (from p in _context.PagoRecibidos
                                      join c in _context.PagoCobroTransitos.Where(c => c.IdCobroTransito == cobro.IdCobroTransito)
                                      on p.IdPagoRecibido equals c.IdPagoRecibido
                                      select p).FirstOrDefaultAsync();

                    if (pago != null)
                    {
                        // proceso de descuento en los recibos y en la propiedad
                        #region PAGO RECIBIENDO CUALQUIER MONTO
                        // PROCESO DE CONFIRMAR PAGO
                        var montoPago = cobro.Monto; // auxiliar para recorrer los recibos con el monto del pago                         

                        if (modelo.Recibos != null && modelo.Recibos.Any())
                        {
                            foreach (var recibo in modelo.Recibos)
                            {
                                decimal pendientePago = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.TotalPagar;

                                if (pendientePago != 0 && pendientePago > montoPago)
                                {
                                    recibo.Abonado += montoPago;
                                    montoPago = 0;
                                }
                                else if (pendientePago != 0 && pendientePago < montoPago)
                                {
                                    recibo.Abonado += montoPago;
                                    recibo.Pagado = true;
                                    montoPago -= pendientePago;
                                }
                                else if (pendientePago != 0 && pendientePago == montoPago)
                                {
                                    recibo.Abonado += montoPago;
                                    recibo.Pagado = true;
                                    montoPago = 0;
                                }

                                var pagoRecibo = new PagosRecibo()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdRecibo = recibo.IdReciboCobro
                                };

                                recibo.TotalPagar = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.Monto + recibo.MontoMora + recibo.MontoIndexacion - recibo.Abonado;
                                recibo.TotalPagar = recibo.TotalPagar < 0 ? 0 : recibo.TotalPagar;

                                _context.ReciboCobros.Update(recibo);
                                _context.PagosRecibos.Add(pagoRecibo);
                            }

                            await _context.SaveChangesAsync();

                            var recibosActualizados = await _context.ReciboCobros
                                .Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();

                            propiedad.Deuda = recibosActualizados
                                .Where(c => !c.Pagado && !c.ReciboActual)
                                .Sum(c => c.TotalPagar);

                            propiedad.Saldo = recibosActualizados
                                            .Where(c => c.ReciboActual)
                                            .Sum(c => c.Monto - c.Abonado);

                            propiedad.Saldo = propiedad.Saldo < 0 ? 0 : propiedad.Saldo;

                            if (montoPago > 0)
                            {
                                propiedad.Creditos += montoPago;
                            }

                            //// VERIFICAR SOLVENCIA DE LA PROPIEDAD
                            if (propiedad.Saldo == 0 && propiedad.Deuda == 0)
                            {
                                propiedad.Solvencia = true;
                            }
                            else
                            {
                                propiedad.Solvencia = false;
                            }

                            _context.Propiedads.Update(propiedad);
                            // modificar cobro en transito
                            cobro.Asignado = true;
                            // crear relacion pago propiedad
                            var pagoPropiedad = new PagoPropiedad()
                            {
                                IdPago = pago.IdPagoRecibido,
                                IdPropiedad = propiedad.IdPropiedad,
                                Confirmado = true,
                                Rectificado = false,
                                Activo = true
                            };

                            _context.PagoPropiedads.Add(pagoPropiedad);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Esta propiedad no tiene recibos pendiente!";

                            TempData.Keep();

                            return View(modelo);
                        }
                        #endregion
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                TempData.Keep();

                return View("Error", modeloError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AjaxCargarRecibos(int valor)
        {
            AsignarCobroTransitoReciboVM modelo = new AsignarCobroTransitoReciboVM();

            if (valor > 0)
            {
                var propiedad = await _context.Propiedads.FindAsync(valor);

                if (propiedad != null)
                {                  

                    var recibos = await (from c in _context.ReciboCobros
                                         where c.IdPropiedad == valor
                                         where !c.Pagado
                                         select c).ToListAsync();

                    modelo.Recibos = recibos;

                    if (modelo.Recibos.Any())
                    {
                        modelo.RecibosModel = recibos.Where(c => !c.EnProceso && !c.Pagado)
                            .Select(c => new SelectListItem { Text = c.Fecha.ToString("dd/MM/yyyy"), Value = c.IdReciboCobro.ToString() })
                            .ToList();
                        
                        modelo.ListRecibos = recibos.Select(recibo => new SelectListItem
                        {
                            Text = recibo.Mes + " " + (recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.TotalPagar).ToString("N") + "Bs",
                            Value = recibo.IdReciboCobro.ToString(),
                            Selected = false,
                        }).ToList();
                    }
                }
            }

            return Json(modelo);
        }

    }
}
