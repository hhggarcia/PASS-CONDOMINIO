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

    public class FacturaEmitidasController : Controller
    {
        private readonly IPrintServices _printServices;
        private readonly IPDFServices _servicesPDF;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IPagosRecibidosRepository _repoPagoRecibido;
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public FacturaEmitidasController(IPrintServices printServices,
            IPDFServices servicesPDF,
            ICuentasContablesRepository repoCuentas,
            IPagosRecibidosRepository repoPagoRecibido,
            IFiltroFechaRepository filtroFechaRepository, 
            NuevaAppContext context)
        {
            _printServices = printServices;
            _servicesPDF = servicesPDF; 
            _repoCuentas = repoCuentas;
            _repoPagoRecibido = repoPagoRecibido;
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: FacturaEmitidas
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var listFacturas = await (from f in _context.FacturaEmitida.Include(f => f.IdClienteNavigation).Include(f => f.IdProductoNavigation)
                                      join c in _context.Clientes 
                                      on f.IdCliente equals c.IdCliente
                                      where c.IdCondominio == IdCondominio
                                      select f).ToListAsync();

            //var nuevaAppContext = _context.FacturaEmitida.Include(f => f.IdProductoNavigation);

            TempData.Keep();

            return View(listFacturas);
        }

        // GET: FacturaEmitidas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaEmitida = await _context.FacturaEmitida
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdFacturaEmitida == id);
            if (facturaEmitida == null)
            {
                return NotFound();
            }

            return View(facturaEmitida);
        }

        // GET: FacturaEmitidas/Create
        public async Task<IActionResult> Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var listFacturas = await(from f in _context.FacturaEmitida
                                     join c in _context.Clientes on f.IdCliente equals c.IdCliente
                                     where c.IdCondominio == IdCondominio
                                     select f).ToListAsync();

            if (listFacturas.Count != 0)
            {
                ViewData["NumFactura"] = listFacturas[listFacturas.Count - 1].NumFactura;
                ViewData["NumControl"] = listFacturas[listFacturas.Count - 1].NumControl;
            }

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(IdCondominio);


            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre");
            ViewData["IdCodCuenta"] = new SelectList(subcuentas.OrderBy(c => c.Descricion), "Id", "Descricion");

            TempData.Keep();
            return View();
        }

        // POST: FacturaEmitidas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFacturaEmitida,IdCliente,IdProducto,IdCodCuenta,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,SubTotal,Iva,MontoTotal,Abonado,Pagada,EnProceso,Anulada")] FacturaEmitida facturaEmitida)
        {
            

            ModelState.Remove(nameof(facturaEmitida.IdProductoNavigation));
            ModelState.Remove(nameof(facturaEmitida.IdClienteNavigation));
            ModelState.Remove(nameof(facturaEmitida.IdCodCuentaNavigation));
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                var idCuenta = _context.SubCuenta.Where(c => c.Id == facturaEmitida.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();

                facturaEmitida.IdCodCuenta = idCodCuenta;
                facturaEmitida.MontoTotal = facturaEmitida.SubTotal + facturaEmitida.Iva;
                facturaEmitida.EnProceso = true;
                _context.Add(facturaEmitida);
                await _context.SaveChangesAsync();

                // calcular retenciones al producto/servicio

                var cliente = await _context.Clientes.FindAsync(facturaEmitida.IdCliente);

                if (cliente == null)
                {
                    return NotFound();
                }


                var rtiva = await _context.Ivas.FindAsync(cliente.IdRetencionIva);

                var rtislr = await _context.Islrs.FindAsync(cliente.IdRetencionIslr);

                decimal montoRTIVA = 0;
                decimal montoRTISLR = 0;


                if (rtiva != null)
                {
                    montoRTIVA = facturaEmitida.Iva * (rtiva.Porcentaje / 100);
                }

                if (rtislr != null)
                {
                    montoRTISLR = facturaEmitida.SubTotal * (rtislr.Tarifa / 100) - rtislr.Sustraendo;
                }

                // actualizar deuda del cliente

                cliente.Deuda += facturaEmitida.MontoTotal - montoRTIVA - montoRTISLR;


                // registrar en libro de ventas

                var itemLibroVenta = new LibroVenta
                {
                    IdCondominio = IdCondominio,
                    IdFactura = facturaEmitida.IdFacturaEmitida,
                    BaseImponible = facturaEmitida.SubTotal,
                    Iva = facturaEmitida.Iva,
                    Total = facturaEmitida.MontoTotal,
                    RetIva = montoRTIVA,
                    RetIslr = montoRTISLR,
                    Monto = facturaEmitida.MontoTotal,
                    NumComprobanteRet = 0,
                    Activo = true,
                    TotalVentaIva = facturaEmitida.MontoTotal,
                    VentaExenta = 0,
                    VentaGravable = 0,
                    IvaRetenido = 0
                };

                // registrar en cuentas por cobrar

                var itemCuentaPorCobrar = new CuentasCobrar
                {
                    IdCondominio = IdCondominio,
                    IdFactura = facturaEmitida.IdFacturaEmitida,
                    Monto = facturaEmitida.MontoTotal,
                    Status = facturaEmitida.EnProceso ? "En Proceso" : "Pagada"                   
                };

                _context.Add(itemLibroVenta);
                _context.Add(itemCuentaPorCobrar);
                _context.Clientes.Update(cliente);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            var subcuentas = await _repoCuentas.ObtenerSubcuentas(IdCondominio);

            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre", facturaEmitida.IdCliente);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", facturaEmitida.IdProducto);
            ViewData["IdCodCuenta"] = new SelectList(subcuentas.OrderBy(c => c.Descricion), "Id", "Descricion", facturaEmitida.IdCodCuenta);

            TempData.Keep();

            return View(facturaEmitida);
        }

        // GET: FacturaEmitidas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaEmitida = await _context.FacturaEmitida.FindAsync(id);
            if (facturaEmitida == null)
            {
                return NotFound();
            }

            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdCodCuenta == facturaEmitida.IdCodCuenta).Select(c => c.IdSubCuenta).FirstOrDefault();

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(IdCondominio);

            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre", facturaEmitida.IdCliente);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", facturaEmitida.IdProducto);
            ViewData["IdCodCuenta"] = new SelectList(subcuentas.OrderBy(c => c.Descricion), "Id", "Descricion", idCodCuenta);

            TempData.Keep();

            return View(facturaEmitida);
        }

        // POST: FacturaEmitidas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFacturaEmitida,IdCliente,IdCodCuenta,IdProducto,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,SubTotal,Iva,MontoTotal,Abonado,Pagada,EnProceso,Anulada")] FacturaEmitida facturaEmitida)
        {
            if (id != facturaEmitida.IdFacturaEmitida)
            {
                return NotFound();
            }

            

            ModelState.Remove(nameof(facturaEmitida.IdProductoNavigation));
            ModelState.Remove(nameof(facturaEmitida.IdClienteNavigation));
            ModelState.Remove(nameof(facturaEmitida.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    var idCuenta = _context.SubCuenta.Where(c => c.Id == facturaEmitida.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                    var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();

                    facturaEmitida.IdCodCuenta = idCodCuenta;
                    facturaEmitida.MontoTotal = facturaEmitida.SubTotal + facturaEmitida.Iva;
                    _context.Update(facturaEmitida);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaEmitidaExists(facturaEmitida.IdFacturaEmitida))
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

            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(IdCondominio);

            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre", facturaEmitida.IdCliente);
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", facturaEmitida.IdProducto);
            ViewData["IdCodCuenta"] = new SelectList(subcuentas.OrderBy(c => c.Descricion), "Id", "Descricion", facturaEmitida.IdCodCuenta);

            TempData.Keep();

            return View(facturaEmitida);
        }

        // GET: FacturaEmitidas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaEmitida = await _context.FacturaEmitida
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdFacturaEmitida == id);
            if (facturaEmitida == null)
            {
                return NotFound();
            }

            return View(facturaEmitida);
        }

        // POST: FacturaEmitidas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facturaEmitida = await _context.FacturaEmitida.FindAsync(id);
            if (facturaEmitida != null)
            {
                var pagosFactura = await _context.PagoFacturaEmitida.Where(c => c.IdFactura.Equals(id)).ToListAsync();
                var itemLibroVenta = await _context.LibroVentas.Where(c => c.IdFactura == facturaEmitida.IdFacturaEmitida).ToListAsync();
                var itemCuentasCobrar = await _context.CuentasCobrars.Where(c => c.IdFactura == facturaEmitida.IdFacturaEmitida).ToListAsync();

                if (pagosFactura != null)
                {
                    _context.PagoFacturaEmitida.RemoveRange(pagosFactura);
                }
                if (itemLibroVenta != null)
                {
                    _context.LibroVentas.RemoveRange(itemLibroVenta);
                }
                if (itemCuentasCobrar != null)
                {
                    _context.CuentasCobrars.RemoveRange(itemCuentasCobrar);
                }
                _context.FacturaEmitida.Remove(facturaEmitida);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Cobros()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await _repoPagoRecibido.GetPagosFacturasEmitidas(idCondominio);

            TempData.Keep();

            return View(modelo);
        }

        public async Task<IActionResult> PagoFactura()
        {
            try
            {
                //traer subcuentas del condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoPagoRecibido.FormPagoFacturaEmitida(idCondominio);

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

        public IActionResult ConfirmarPago(PagoFacturaEmitidaVM modelo)
        {
            return View(modelo);
        }

        public IActionResult Comprobante(ComprobantePagoFacEmitidaVM modelo)
        {
            return View(modelo);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RegistrarPagosPost(PagoFacturaEmitidaVM modelo)
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

                            modelo = await _repoPagoRecibido.FormPagoFacturaEmitida(modelo.IdCondominio);

                            TempData.Keep();

                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe una transferencia con este número de referencia!";

                            return View("PagoFactura", modelo);
                        }
                    }

                    var resultado = await _repoPagoRecibido.RegistrarPago(modelo);

                    if (resultado == "exito")
                    {
                        var factura = await _context.FacturaEmitida.Where(c => c.IdFacturaEmitida == modelo.IdFactura).FirstAsync();
                        var condominio = await _context.Condominios.FindAsync(modelo.IdCondominio);

                        var idSubCuenta = (from c in _context.CodigoCuentasGlobals
                                           join f in _context.FacturaEmitida
                                           on c.IdCodCuenta equals f.IdCodCuenta
                                           where f.IdFacturaEmitida == modelo.IdFactura
                                           select c.IdSubCuenta).First();

                        var gasto = from c in _context.SubCuenta
                                    where c.Id == idSubCuenta
                                    select c;

                        var comprobante = new ComprobantePagoFacEmitidaVM()
                        {
                            Condominio = condominio,
                            Concepto = modelo.Concepto,
                            Pagoforma = modelo.Pagoforma,
                            Mensaje = "¡Gracias por su pago!",
                            Gasto = gasto.First()
                        };

                        if (modelo.Pagoforma == FormaPago.Transferencia)
                        {
                            var banco = from c in _context.SubCuenta
                                        where c.Id == modelo.IdCodigoCuentaBanco
                                        select c;

                            comprobante.Banco = banco.First();
                            comprobante.NumReferencia = modelo.NumReferencia;

                        }
                        else
                        {
                            var caja = from c in _context.SubCuenta
                                       where c.Id == modelo.IdCodigoCuentaCaja
                                       select c;

                            comprobante.Caja = caja.First();
                        }

                        var cliente = await _context.Clientes.Where(c => c.IdCliente == factura.IdCliente).FirstAsync();
                        var retIslr = _context.Islrs.Where(c => c.Id == cliente.IdRetencionIslr).FirstOrDefault();
                        var retIva = _context.Ivas.Where(c => c.Id == cliente.IdRetencionIva).FirstOrDefault();

                        comprobante.Factura = factura;

                        if (retIslr != null)
                        {
                            comprobante.Islr = (factura.SubTotal * (retIslr.Tarifa / 100)) - retIslr.Sustraendo;
                        }

                        if (retIva != null)
                        {
                            comprobante.Iva = factura.Iva * (retIva.Porcentaje / 100);
                        }
                        comprobante.Pago.Monto = modelo.Monto;
                        comprobante.Pago.Fecha = modelo.Fecha;
                        comprobante.Pago.ValorDolar = modelo.ValorDolar;
                        comprobante.Pago.MontoRef = modelo.MontoRef;
                        comprobante.Pago.SimboloRef = modelo.SimboloRef;
                        comprobante.Pago.SimboloMoneda = modelo.SimboloMoneda;
                        comprobante.Beneficiario = cliente.Nombre;
                        comprobante.RetencionesIslr = modelo.RetencionesIslr;
                        comprobante.RetencionesIva = modelo.RetencionesIva;

                        foreach (var item in condominio.MonedaConds)
                        {
                            comprobante.ValorDolar = item.ValorDolar;
                        }

                        TempData.Keep();

                        return View("Comprobante", comprobante);
                    }

                    ViewBag.FormaPago = "fallido";
                    ViewBag.Mensaje = resultado;
                    //traer subcuentas del condominio
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    modelo = await _repoPagoRecibido.FormPagoFacturaEmitida(idCondominio);

                    TempData.Keep();

                    return View("PagoFactura", modelo);

                }
                //traer subcuentas del condominio
                var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo = await _repoPagoRecibido.FormPagoFacturaEmitida(id);

                TempData.Keep();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "Ha ocurrido un error inesperado";

                return View("PagoFactura", modelo);

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
        private bool FacturaEmitidaExists(int id)
        {
            return _context.FacturaEmitida.Any(e => e.IdFacturaEmitida == id);
        }

        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var facturasEmitdas = await _reposFiltroFecha.ObteneFactirasEmitidas(filtrarFechaVM);
            return View("Index", facturasEmitdas);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFacturasPorCliente(int id)
        {
            // Lógica para obtener las facturas asociadas al proveedor seleccionado
            var facturas = await _context.FacturaEmitida
           .Where(c => c.IdCliente == id && c.EnProceso == true)
           .ToListAsync();

            // Devolver las facturas en formato JSON
            var facturaItems = facturas.Select(f => new { Value = f.IdFacturaEmitida, Text = f.NumFactura }).ToList();
            return Json(facturaItems);
        }

        public async Task<IActionResult> ObtenerFactura(int id)
        {
            var factura = await _context.FacturaEmitida.Where(c => c.IdFacturaEmitida == id).FirstAsync();

            var facturaMonto = new
            {
                Value = factura.IdFacturaEmitida,
                Monto = factura.MontoTotal - factura.Abonado,
                Descripcion = factura.Descripcion
            };

            return Json(facturaMonto);
        }

        public async Task<IActionResult> FacturaEmitidaPDF(int id)
        {
            var factura = await _context.FacturaEmitida.FindAsync(id);

            if (factura != null)
            {
                var data = await _servicesPDF.FacturaDeVentaPDF(factura);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "FacturaVenta.pdf");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PrintFacturaEmitida(int id)
        {
            var factura = await _context.FacturaEmitida.FindAsync(id);
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (factura != null)
            {
                var data = await _servicesPDF.FacturaDeVentaPDF(factura);
                var resultado = _printServices.PrintCompRetencion(data, idCondominio);
                //Stream stream = new MemoryStream(data);
                //return File(stream, "application/pdf", "Recibo.pdf");
                TempData.Keep();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del pago recibido por el cliente</param>
        /// <returns></returns>
        public async Task<IActionResult> CobroPDF(int id)
        {
            var pago = await _context.PagoRecibidos.FindAsync(id);

            if (pago != null)
            {
                var data = await _servicesPDF.ComprobantePagoRecibidoClientePDF(pago);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "CompCliente.pdf");
            }
            return RedirectToAction("Index");
        }
    }
}
