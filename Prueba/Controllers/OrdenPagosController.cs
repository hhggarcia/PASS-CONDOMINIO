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

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class OrdenPagosController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly IPagosEmitidosRepository _repoPagosEmitidos;
        private readonly NuevaAppContext _context;

        public OrdenPagosController(IPDFServices servicesPDF,
            IPagosEmitidosRepository repoPagosEmitidos,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
            _repoPagosEmitidos = repoPagosEmitidos;
            _context = context;
        }

        // GET: OrdenPagos
        public async Task<IActionResult> Index()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.OrdenPagos
                .Include(o => o.IdPagoEmitidoNavigation)
                .Include(o => o.IdProveedorNavigation)
                .Where(c => c.IdProveedorNavigation.IdCondominio == idCondominio);

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: OrdenPagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenPago = await _context.OrdenPagos
                .Include(o => o.IdPagoEmitidoNavigation)
                .Include(o => o.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdOrdenPago == id);
            if (ordenPago == null)
            {
                return NotFound();
            }

            return View(ordenPago);
        }

        // GET: OrdenPagos/Create
        public IActionResult Create()
        {
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: OrdenPagos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrdenPago,IdPagoEmitido,IdProveedor,Fecha")] OrdenPago ordenPago)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ordenPago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido", ordenPago.IdPagoEmitido);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", ordenPago.IdProveedor);
            return View(ordenPago);
        }

        // GET: OrdenPagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenPago = await _context.OrdenPagos.FindAsync(id);
            if (ordenPago == null)
            {
                return NotFound();
            }
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido", ordenPago.IdPagoEmitido);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", ordenPago.IdProveedor);
            return View(ordenPago);
        }

        // POST: OrdenPagos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdOrdenPago,IdPagoEmitido,IdProveedor,Fecha")] OrdenPago ordenPago)
        {
            if (id != ordenPago.IdOrdenPago)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordenPago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenPagoExists(ordenPago.IdOrdenPago))
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
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido", ordenPago.IdPagoEmitido);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", ordenPago.IdProveedor);
            return View(ordenPago);
        }

        // GET: OrdenPagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenPago = await _context.OrdenPagos
                .Include(o => o.IdPagoEmitidoNavigation)
                .Include(o => o.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdOrdenPago == id);
            if (ordenPago == null)
            {
                return NotFound();
            }

            return View(ordenPago);
        }

        // POST: OrdenPagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordenPago = await _context.OrdenPagos.FindAsync(id);
            if (ordenPago != null)
            {
                _context.OrdenPagos.Remove(ordenPago);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ConfirmarPago(OrdenPagoVM modelo)
        {
            return View(modelo);
        }

        public IActionResult Comprobante(OrdenPagoVM modelo)
        {
            return View(modelo);
        }
        public async Task<IActionResult> OrdenPago()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            // cargar formulario
            var modelo = await _repoPagosEmitidos.FormOrdenPago(idCondominio);
            // usar el repositorio?

            return View(modelo);
        }
        
        public async Task<IActionResult> RegistrarPagosPost(OrdenPagoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {
                    modelo.Pago.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    // validar transferencia
                    if (modelo.Pagoforma == FormaPago.Transferencia)
                    {
                        var existPagoTransferencia = from pago in _context.PagoEmitidos
                                                     join referencia in _context.ReferenciasPes
                                                     on pago.IdPagoEmitido equals referencia.IdPagoEmitido
                                                     where pago.IdCondominio == modelo.Pago.IdCondominio
                                                     where referencia.NumReferencia == modelo.NumReferencia
                                                     select new { pago, referencia };

                        if (existPagoTransferencia != null && existPagoTransferencia.Any())
                        {
                            //var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                            modelo = await _repoPagosEmitidos.FormOrdenPago(modelo.Pago.IdCondominio);

                            TempData.Keep();

                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe una transferencia con este número de referencia!";

                            return View("OrdenPago", modelo);
                        }
                    }

                    // validar monto 

                    var resultado = await _repoPagosEmitidos.RegistrarOrdenPago(modelo);

                    if (resultado == "exito")
                    {
                        var condominio = await _context.Condominios.FindAsync(modelo.Pago.IdCondominio);

                        var idSubCuenta = (from c in _context.CodigoCuentasGlobals
                                           where c.IdCodCuenta == modelo.IdSubcuenta
                                           select c.IdSubCuenta).First();

                        var gasto = from c in _context.SubCuenta
                                    where c.Id == idSubCuenta
                                    select c;

                        var comprobante = new ComprobanteOrdenPago()
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
                        var proveedor = await _context.Proveedors.Where(c => c.IdProveedor == modelo.IdProveedor).FirstAsync();

                        comprobante.Pago.Monto = modelo.Pago.Monto;
                        comprobante.Pago.Fecha = modelo.Pago.Fecha;
                        comprobante.Beneficiario = proveedor.Nombre;

                        TempData.Keep();

                        return View("Comprobante", comprobante);
                    }

                    ViewBag.FormaPago = "fallido";
                    ViewBag.Mensaje = resultado;
                    //traer subcuentas del condominio
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    modelo = await _repoPagosEmitidos.FormOrdenPago(idCondominio);

                    TempData.Keep();

                    return View("OrdenPago", modelo);

                }
                //traer subcuentas del condominio
                var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo = await _repoPagosEmitidos.FormOrdenPago(id);

                TempData.Keep();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "Ha ocurrido un error inesperado";

                return View("OrdenPago", modelo);

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
        private bool OrdenPagoExists(int id)
        {
            return _context.OrdenPagos.Any(e => e.IdOrdenPago == id);
        }

        [HttpPost]
        public ContentResult ComprobantePDF([FromBody] ComprobanteOrdenPago modelo)
        {
            try
            {
                var data = _servicesPDF.ComprobanteOrdenPagoPDF(modelo);
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
    }
}
