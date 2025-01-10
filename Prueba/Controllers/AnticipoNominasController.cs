using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public class AnticipoNominasController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly IPagosEmitidosRepository _repoPagosEmitidos;
        private readonly NuevaAppContext _context;

        public AnticipoNominasController(IPDFServices servicesPDF,
            IPagosEmitidosRepository repoPagosEmitidos,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
            _repoPagosEmitidos = repoPagosEmitidos;
            _context = context;
        }

        //// GET: AnticipoNominas
        //public async Task<IActionResult> Index()
        //{
        //    var nuevaAppContext = _context.AnticipoNominas.Include(a => a.IdEmpleadoNavigation);
        //    return View(await nuevaAppContext.ToListAsync());
        //}
        // GET: AnticipoNominas
        public async Task<IActionResult> Index(int id)
        {
            var nuevaAppContext = _context.AnticipoNominas
                .Include(b => b.IdEmpleadoNavigation)
                .Include(b => b.IdPagoEmitidoNavigation)
                .Where(c => c.IdEmpleado == id);

            return View(await nuevaAppContext.ToListAsync());
        }


        // GET: AnticipoNominas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipoNomina = await _context.AnticipoNominas
                .Include(a => a.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipoNomina == id);
            if (anticipoNomina == null)
            {
                return NotFound();
            }

            return View(anticipoNomina);
        }

        // GET: AnticipoNominas/Create
        public IActionResult Create()
        {
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre");
            return View();
        }

        // POST: AnticipoNominas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAnticipoNomina,IdEmpleado,Monto,Fecha,Activo")] AnticipoNomina anticipoNomina)
        {
            ModelState.Remove(nameof(anticipoNomina.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(anticipoNomina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", anticipoNomina.IdEmpleado);
            return View(anticipoNomina);
        }

        // GET: AnticipoNominas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipoNomina = await _context.AnticipoNominas.FindAsync(id);
            if (anticipoNomina == null)
            {
                return NotFound();
            }
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", anticipoNomina.IdEmpleado);
            return View(anticipoNomina);
        }

        // POST: AnticipoNominas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAnticipoNomina,IdEmpleado,Monto,Fecha,Activo")] AnticipoNomina anticipoNomina)
        {
            if (id != anticipoNomina.IdAnticipoNomina)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(anticipoNomina.IdEmpleadoNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anticipoNomina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnticipoNominaExists(anticipoNomina.IdAnticipoNomina))
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
            ViewData["IdEmpleado"] = new SelectList(_context.Empleados, "IdEmpleado", "Nombre", anticipoNomina.IdEmpleado);
            return View(anticipoNomina);
        }

        // GET: AnticipoNominas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anticipoNomina = await _context.AnticipoNominas
                .Include(a => a.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(m => m.IdAnticipoNomina == id);
            if (anticipoNomina == null)
            {
                return NotFound();
            }

            return View(anticipoNomina);
        }

        // POST: AnticipoNominas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anticipoNomina = await _context.AnticipoNominas.FindAsync(id);
            if (anticipoNomina != null)
            {
                _context.AnticipoNominas.Remove(anticipoNomina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnticipoNominaExists(int id)
        {
            return _context.AnticipoNominas.Any(e => e.IdAnticipoNomina == id);
        }

        public IActionResult ConfirmarPago(PagoAnticipoNominaVM modelo)
        {
            return View(modelo);
        }
        public async Task<IActionResult> RegistrarAnticipoNomina()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            // cargar formulario
            var modelo = await _repoPagosEmitidos.FormPagoAnticipoNomina(idCondominio);
            // usar el repositorio?
            TempData.Keep();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarAnticipoNomina(PagoAnticipoNominaVM modelo)
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
                            modelo = await _repoPagosEmitidos.FormPagoAnticipoNomina(modelo.IdCondominio);

                            TempData.Keep();

                            ViewBag.FormaPago = "fallido";
                            ViewBag.Mensaje = "Ya existe una transferencia con este número de referencia!";

                            return View("RegistrarAnticipoNomina", modelo);
                        }
                    }

                    var resultado = await _repoPagosEmitidos.RegistrarPagoAnticipoNomina(modelo);

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

                        var empleado = await _context.Empleados.Where(c => c.IdEmpleado == modelo.IdEmpleado).FirstAsync();

                        comprobante.Pago.Monto = modelo.Monto;
                        comprobante.Pago.Fecha = modelo.Fecha;
                        comprobante.Beneficiario = empleado.Nombre;

                        TempData.Keep();

                        return View("Comprobante", comprobante);
                    }

                    ViewBag.FormaPago = "fallido";
                    ViewBag.Mensaje = resultado;
                    //traer subcuentas del condominio
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    modelo = await _repoPagosEmitidos.FormPagoAnticipoNomina(idCondominio);

                    TempData.Keep();

                    return View("RegistrarAnticipoNomina", modelo);

                }
                //traer subcuentas del condominio
                var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo = await _repoPagosEmitidos.FormPagoAnticipoNomina(id);

                TempData.Keep();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "Ha ocurrido un error inesperado";

                return View("RegistrarAnticipoNomina", modelo);

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
