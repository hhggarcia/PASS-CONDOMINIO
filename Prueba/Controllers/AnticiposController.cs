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
    public class AnticiposController : Controller
    {
        private readonly NuevaAppContext _context;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IPDFServices _servicesPDF;
        private readonly IPagosEmitidosRepository _repoPagosEmitidos;
        private readonly IFiltroFechaRepository _reposFiltroFecha;


        public AnticiposController(ICuentasContablesRepository repoCuentas,
            IPDFServices servicesPDF,
            IPagosEmitidosRepository repoPagosEmitidos,
            IFiltroFechaRepository filtroFechaRepository, 
            NuevaAppContext context)
        {
            _repoCuentas = repoCuentas;
            _servicesPDF = servicesPDF;
            _repoPagosEmitidos = repoPagosEmitidos;
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: Anticipos
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var listAnticipos = await (from a in _context.Anticipos.Include(f => f.IdProveedorNavigation)
                                      join c in _context.CodigoCuentasGlobals on a.IdCodCuenta equals c.IdCodCuenta
                                      where c.IdCondominio == IdCondominio
                                      select a).ToListAsync();

            //var nuevaAppContext = _context.Anticipos.Include(a => a.IdProveedorNavigation);
            return View(listAnticipos);
        }

        // GET: Anticipos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipo = await _context.Anticipos
                .Include(a => a.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipo == id);
            if (anticipo == null)
            {
                return NotFound();
            }

            return View(anticipo);
        }

        // GET: Anticipos/Create
        public async Task<IActionResult> Create()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre");
            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion");
            TempData.Keep();

            return View();
        }

        // POST: Anticipos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAnticipo,Numero,Fecha,Saldo,Detalle,IdProveedor, IdCodCuenta")] Anticipo anticipo)
        {
            var idCuenta = _context.SubCuenta.Where(c => c.Id == anticipo.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
            anticipo.IdCodCuenta = idCodCuenta;
            anticipo.Activo = true;

            ModelState.Remove(nameof(anticipo.IdProveedorNavigation));
            ModelState.Remove(nameof(anticipo.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(anticipo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", anticipo.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", anticipo.IdCodCuenta);

            TempData.Keep();

            return View(anticipo);
        }

        // GET: Anticipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipo = await _context.Anticipos.FindAsync(id);
            if (anticipo == null)
            {
                return NotFound();
            }
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdCodCuenta == anticipo.IdCodCuenta).Select(c => c.IdSubCuenta).FirstOrDefault();

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", anticipo.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", idCodCuenta);

            TempData.Keep();

            return View(anticipo);
        }

        // POST: Anticipos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAnticipo,Numero,Fecha,Saldo,Detalle,IdProveedor")] Anticipo anticipo)
        {
            if (id != anticipo.IdAnticipo)
            {
                return NotFound();
            }

           

            ModelState.Remove(nameof(anticipo.IdProveedorNavigation));
            ModelState.Remove(nameof(anticipo.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    var idCuenta = _context.SubCuenta.Where(c => c.Id == anticipo.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
                    var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
                    anticipo.IdCodCuenta = idCodCuenta;
                    anticipo.Activo = true;
                    _context.Update(anticipo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnticipoExists(anticipo.IdAnticipo))
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
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(idCondominio);

            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", anticipo.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(subcuentas, "Id", "Descricion", anticipo.IdCodCuenta);

            TempData.Keep();
            return View(anticipo);
        }

        // GET: Anticipos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipo = await _context.Anticipos
                .Include(a => a.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipo == id);
            if (anticipo == null)
            {
                return NotFound();
            }

            return View(anticipo);
        }

        // POST: Anticipos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anticipo = await _context.Anticipos.FindAsync(id);
            if (anticipo != null)
            {
                var pagosAnticipo = await _context.PagoFacturas.Where(c => c.IdAnticipo.Equals(id)).ToListAsync();

                if (pagosAnticipo != null)
                {
                    _context.PagoFacturas.RemoveRange(pagosAnticipo);
                }

                _context.Anticipos.Remove(anticipo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnticipoExists(int id)
        {
            return _context.Anticipos.Any(e => e.IdAnticipo == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> PagoAnticipo()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            // cargar formulario
            var modelo = await _repoPagosEmitidos.FormPagoAnticicipo(idCondominio);
            // usar el repositorio?
            TempData.Keep();
            return View(modelo);
        }

        public IActionResult ConfirmarPago(PagoAnticipoVM modelo)
        {
            return View(modelo);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RegistrarPagosPost(PagoAnticipoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {
                    modelo.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    if (modelo.Pagoforma == FormaPago.Transferencia)
                    {
                        var existPagoTransferencia = from pago in _context.PagoEmitidos
                                                     join referencia in _context.ReferenciasPes
                                                     on pago.IdPagoEmitido equals referencia.IdPagoEmitido
                                                     where pago.IdCondominio == modelo.IdCondominio
                                                     where referencia.NumReferencia == modelo.NumReferencia
                                                     select new { pago, referencia };

                        if (existPagoTransferencia != null && existPagoTransferencia.Any())
                        {
                            modelo = await _repoPagosEmitidos.FormPagoAnticicipo(modelo.IdCondominio);

                            TempData.Keep();

                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe una transferencia con este número de referencia!";

                            return View("PagoAnticipo", modelo);
                        }
                    }

                    var resultado = await _repoPagosEmitidos.RegistrarAnticipo(modelo);

                    if (resultado == "exito")
                    {
                        var condominio = await _context.Condominios.FindAsync(modelo.IdCondominio);


                        var comprobante = new ComprobanteAnticipoVM()
                        {
                            Condominio = condominio,
                            Concepto = modelo.Concepto,
                            Pagoforma = modelo.Pagoforma,
                            Mensaje = "¡Gracias por su pago!"
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

                        comprobante.Pago.Monto = modelo.Monto;
                        comprobante.Pago.Fecha = modelo.Fecha;
                        comprobante.Beneficiario = proveedor.Nombre;

                        TempData.Keep();

                        return View("Comprobante", comprobante);
                    }

                    ViewBag.FormaPago = "fallido";
                    ViewBag.Mensaje = resultado;
                    //traer subcuentas del condominio
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    modelo = await _repoPagosEmitidos.FormPagoAnticicipo(idCondominio);

                    TempData.Keep();

                    return View("PagoAnticipo", modelo);

                }
                //traer subcuentas del condominio
                var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo = await _repoPagosEmitidos.FormPagoAnticicipo(id);

                TempData.Keep();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "Ha ocurrido un error inesperado";

                return View("PagoAnticipo", modelo);

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
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var filtrarFecha = await _reposFiltroFecha.ObtenerAnticipos(filtrarFechaVM);
            return View("Index", filtrarFecha);
        }

        public ContentResult ComprobantePDF([FromBody] ComprobanteAnticipoVM modelo)
        {
            try
            {
                var data = _servicesPDF.ComprobanteAnticipoPDF(modelo);
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
