using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class PagosEmitidosController : Controller
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly IPagosEmitidosRepository _repoPagosEmitidos;
        private readonly IPDFServices _servicePDF;
        private readonly NuevaAppContext _context;

        public PagosEmitidosController(IMonedaRepository repoMoneda,
            IPagosEmitidosRepository repoPagosEmitidos,
            IPDFServices servicePDF,
            NuevaAppContext context)
        {
            _repoMoneda = repoMoneda;
            _repoPagosEmitidos = repoPagosEmitidos;
            _servicePDF = servicePDF;
            _context = context;
        }

        // GET: PagosEmitidos
        //public async Task<IActionResult> Index()
        //{
        //    var NuevaAppContext = _context.PagoEmitidos.Include(p => p.IdCondominioNavigation);
        //    return View(await NuevaAppContext.ToListAsync());
        //}

        public async Task<IActionResult> Index()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString()); 

            var modelo = await _repoPagosEmitidos.GetPagosEmitidos(idCondominio);

            TempData.Keep();

            return View(modelo);
        }

        // GET: PagosEmitidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PagoEmitidos == null)
            {
                return NotFound();
            }

            var pagoEmitido = await _context.PagoEmitidos
                .Include(p => p.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoEmitido == id);
            if (pagoEmitido == null)
            {
                return NotFound();
            }

            return View(pagoEmitido);
        }

        // GET: PagosEmitidos/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            //ViewData["IdDolar"] = new SelectList(_context.ReferenciaDolars, "IdReferencia", "Valor");

            return View();
        }

        // POST: PagosEmitidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPagoEmitido,IdCondominio,IdProveedor,Fecha,Monto,MontoRef,IdMoneda,FormaPago")] PagoEmitido pagoEmitido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagoEmitido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoEmitido.IdCondominio);

            return View(pagoEmitido);
        }

        // GET: PagosEmitidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PagoEmitidos == null)
            {
                return NotFound();
            }

            var pagoEmitido = await _context.PagoEmitidos.FindAsync(id);
            if (pagoEmitido == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoEmitido.IdCondominio);
            ViewData["IdMoneda"] = new SelectList(_context.MonedaConds, "Simbolo", "Simbolo");

            return View(pagoEmitido);
        }

        // POST: PagosEmitidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPagoEmitido,IdCondominio,IdProveedor,Fecha,Monto,MontoRef,FormaPago,ValorDolar,SimboloMoneda,SimboloRef")] PagoEmitido pagoEmitido)
        {
            if (id != pagoEmitido.IdPagoEmitido)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                // cambiar montoRef, simbolo ref, valor dolar si cambiaron el tipo de moneda
                // buscar moneda
                //var moneda = await _context.MonedaConds.Where(c => c.Simbolo == pagoEmitido.SimboloMoneda).ToListAsync();
                decimal montoReferencia = 0;

                var moneda = from c in _context.MonedaConds
                             where c.Simbolo == pagoEmitido.SimboloMoneda
                             select c;

                // buscar principal
                var monedaPrincipal = await _repoMoneda.MonedaPrincipal(pagoEmitido.IdCondominio);

                if (!monedaPrincipal.Any())
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Debe existir por lo menos una moneda principal!"
                    };

                    return View("Error", modeloError);
                }

                // cambiar montos
                if (moneda.First().Equals(monedaPrincipal.First()))
                {
                    montoReferencia = pagoEmitido.Monto;

                }
                else if (!moneda.First().Equals(monedaPrincipal.First()))
                {
                    var montoDolares = pagoEmitido.Monto * moneda.First().ValorDolar;

                    montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                }

                pagoEmitido.MontoRef = montoReferencia;
                pagoEmitido.SimboloRef = monedaPrincipal.First().Simbolo;

                _context.Update(pagoEmitido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repoPagosEmitidos.PagoEmitidoExists(pagoEmitido.IdPagoEmitido))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            //}
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", pagoEmitido.IdCondominio);
            //return View(pagoEmitido);
        }

        // GET: PagosEmitidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || _context.PagoEmitidos == null)
                {
                    return NotFound();
                }

                var pagoEmitido = await _context.PagoEmitidos
                    .Include(p => p.IdCondominioNavigation)
                    .FirstOrDefaultAsync(m => m.IdPagoEmitido == id);
                if (pagoEmitido == null)
                {
                    return NotFound();
                }

                return View(pagoEmitido);
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

        // POST: PagosEmitidos/Delete/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.PagoEmitidos == null)
                {
                    return Problem("Entity set 'NuevaAppContext.PagoEmitidos'  is null.");
                }
                var result = await _repoPagosEmitidos.Delete(id);

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

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RegistrarPagos()
        {
            try
            {
                //traer subcuentas del condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoPagosEmitidos.FormRegistrarPago(idCondominio);

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

        public IActionResult ConfirmarPago(RegistroPagoVM modelo)
        {
            return View(modelo);
        }

        public IActionResult Comprobante(ComprobantePEVM modelo)
        {
            return View(modelo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RegistrarPagosPost(RegistroPagoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {
                    modelo.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    //if (ModelState.IsValid)
                    //{
                    var beneficiario = await _context.Condominios.Where(c => c.IdCondominio == modelo.IdCondominio).Select(c => c.Nombre).FirstAsync();
                    if (modelo.Pagoforma == FormaPago.Transferencia)
                    {
                        var existPagoTransferencia = from pago in _context.PagoEmitidos
                                                     join referencia in _context.ReferenciasPes
                                                     on pago.IdPagoEmitido equals referencia.IdPagoEmitido
                                                     where pago.IdCondominio == modelo.IdCondominio
                                                     where referencia.NumReferencia == modelo.NumReferencia
                                                     select new {  pago, referencia };

                        if (existPagoTransferencia != null && existPagoTransferencia.Any())
                        {
                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = "Ya existe un pago registrado con este número de referencia!"
                            };

                            return View("Error", modeloError);
                        }
                    }

                    var resultado = await _repoPagosEmitidos.RegistrarPago(modelo);

                    if (resultado) 
                    {
                        var factura = await _context.Facturas.Where(c=>c.IdFactura == modelo.IdFactura).FirstAsync();    
                        var condominio = await _context.Condominios.FindAsync(modelo.IdCondominio);

                        var idSubCuenta = (from c in _context.CodigoCuentasGlobals
                                          join f in _context.Facturas
                                          on c.IdCodCuenta equals f.IdCodCuenta
                                          where f.IdFactura == modelo.IdFactura
                                          select c.IdSubCuenta).First();

                        var gasto = from c in _context.SubCuenta
                                    where c.Id == idSubCuenta
                                    select c;       

                        var comprobante = new ComprobantePEVM()
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
                        if(modelo.IdAnticipo!=0)
                        {
                            var anticipo = await _context.Anticipos.Where(c => c.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                            comprobante.Anticipo = anticipo;
                        }
                        comprobante.Factura = factura;
                        comprobante.Islr = modelo.RetIslr;
                        comprobante.Iva = modelo.RetIva;
                        comprobante.Pago.Monto = modelo.Monto;
                        comprobante.Pago.Fecha = modelo.Fecha;
                        comprobante.Pago.ValorDolar = modelo.ValorDolar;
                        comprobante.Pago.MontoRef = modelo.MontoRef;
                        comprobante.Pago.SimboloRef = modelo.SimboloRef;
                        comprobante.Pago.SimboloMoneda = modelo.SimboloMoneda;
                        comprobante.Beneficiario = beneficiario;
                        foreach(var item in condominio.MonedaConds)
                        {
                            comprobante.ValorDolar = item.ValorDolar;
                        }
                        TempData.Keep();
                        return View("Comprobante", comprobante);
                    }

                    //}
                    ViewBag.FormaPago = "fallido";
                    //traer subcuentas del condominio
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    modelo = await _repoPagosEmitidos.FormRegistrarPago(idCondominio);
                    
                    TempData.Keep();

                    return View("RegistrarPagos", modelo);

                }
                //traer subcuentas del condominio
                var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo = await _repoPagosEmitidos.FormRegistrarPago(id);

                TempData.Keep();

                ViewBag.FormaPago = "fallido";

                return View("RegistrarPagos", modelo);

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
        //[ValidateAntiForgeryToken]
        //public IActionResult ComprobantePdf(ComprobantePEVM comprobante)
        [HttpPost]
        public ContentResult ComprobantePDF([FromBody] ComprobantePEVM comprobanteVM)
        {
            try
            {
                var data = _servicePDF.ComprobantePEVMPDF(comprobanteVM);
                var base64 = Convert.ToBase64String(data);
                return Content(base64, "application/pdf");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generando PDF: {e.Message}");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
            }
        }

        [HttpPost]
        public ContentResult PagosEmitidosPDF([FromBody] IndexPagosVM comprobanteVM)
        {
            try
            {
                var data = _servicePDF.PagosEmitidosPDF(comprobanteVM);
                var base64 = Convert.ToBase64String(data);
                return Content(base64, "application/pdf");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generando PDF: {e.Message}");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> ObtenerFacturasPorProveedor(int proveedorId)
        {
            // Lógica para obtener las facturas asociadas al proveedor seleccionado
            var facturas = await _context.Facturas
           .Where(c => c.IdProveedor == proveedorId && c.EnProceso == true)
           .ToListAsync();

            // Devolver las facturas en formato JSON
            var facturaItems = facturas.Select(f => new { Value = f.IdFactura, Text = f.NumFactura }).ToList();
            return Json(facturaItems);
        }
        public async Task<IActionResult> ObtenerFactura(int facturaId)
        {
            var factura = await _context.Facturas.Where(c => c.IdFactura == facturaId).FirstAsync();

            var itemLibroCompra = await _context.LibroCompras.Where(c => c.IdFactura == factura.IdFactura).FirstOrDefaultAsync();

            if (itemLibroCompra != null)
            {
                factura.MontoTotal -= itemLibroCompra.RetIva + itemLibroCompra.RetIslr;
            }
            //Console.WriteLine(itemLibroCompra.RetIva + " " + itemLibroCompra.RetIslr);

            var facturaMonto = new
            {
                Value = factura.IdFactura,
                Monto = factura.MontoTotal,
                Iva = itemLibroCompra.RetIva,
                Islr = itemLibroCompra.RetIslr,
                Descripcion = factura.Descripcion
            };

            return Json(facturaMonto);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerAnticiposPorProveedor(int proveedorId)
        {
            var anticipos = await _context.Anticipos
           .Where(c => c.IdProveedor == proveedorId && c.Activo != false)
           .ToListAsync();

            var anticiposItems = anticipos.Select(f => new { Value = f.IdAnticipo, Text = "Ref. "+f.Numero + " Saldo " + f.Saldo + " Bs" , Precio = f.Saldo}).ToList();
            return Json(anticiposItems);
        }

    }
}