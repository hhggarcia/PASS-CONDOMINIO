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

    public class EmpleadosController : Controller
    {
        private readonly IPDFServices _servicesPDF;
        private readonly IPagosEmitidosRepository _repoPagosEmitidos;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public EmpleadosController(IPDFServices servicesPDF,
            IPagosEmitidosRepository repoPagosEmitidos,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _servicesPDF = servicesPDF;
            _repoPagosEmitidos = repoPagosEmitidos;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        // GET: Empleados
        public async Task<IActionResult> Index()
        {
            return View(await _context.Empleados.ToListAsync());
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empleados/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEmpleado,Nombre,Apellido,Cedula,FechaIngreso,Estado,BaseSueldo,RefMonto")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var monedaPrincipal = (await _repoMoneda.MonedaPrincipal(idCondominio)).FirstOrDefault();

                if (monedaPrincipal != null)
                {
                    empleado.RefMonto = empleado.BaseSueldo / monedaPrincipal.ValorDolar;
                }
                _context.Add(empleado);
                await _context.SaveChangesAsync();

                // registrar en nomina condomino
                var registro = new CondominioNomina
                {
                    IdCondominio = idCondominio,
                    IdEmpleado = empleado.IdEmpleado
                };

                _context.CondominioNominas.Add(registro);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEmpleado,Nombre,Apellido,Cedula,FechaIngreso,Estado,BaseSueldo,RefMonto")] Empleado empleado)
        {
            if (id != empleado.IdEmpleado)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.IdEmpleado))
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
            return View(empleado);
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                var deducciones = _context.Deducciones.Where(d => d.IdEmpleado == id).ToList();
                var percepciones = _context.Percepciones.Where(d => d.IdEmpleado == id).ToList();
                var condNomina = _context.CondominioNominas.Where(d => d.IdEmpleado == id).ToList();

                if (deducciones.Any())
                {
                    _context.Deducciones.RemoveRange(deducciones);
                }

                if (percepciones.Any())
                {
                    _context.Percepciones.RemoveRange(percepciones);
                }

                if (condNomina.Any())
                {
                    _context.CondominioNominas.RemoveRange(condNomina);
                }

                var recibos = _context.ReciboNominas.Where(d => d.IdEmpleado == id).ToList();

                if (recibos.Any())
                {
                    foreach (var recibo in recibos)
                    {
                        var pagosRecibo = _context.PagosNominas.Where(d => d.IdReciboNomina == recibo.IdReciboNomina).ToList();

                        _context.PagosNominas.RemoveRange(pagosRecibo);
                    }

                    _context.ReciboNominas.RemoveRange(recibos);
                }

                _context.Empleados.Remove(empleado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.IdEmpleado == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del empleado a consultar deducciones</param>
        /// <returns></returns>
        public IActionResult VerDeducciones(int id)
        {
            return RedirectToAction("Index", "Deducciones", new { id });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del empleado a consultar percepciones</param>
        /// <returns></returns>
        public IActionResult VerPercepciones(int id)
        {
            return RedirectToAction("Index", "Percepciones", new { id });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">Id del empleado a consultar recibos de nomina</param>
        /// <returns></returns>
        public IActionResult VerRecibosNomina(int id)
        {
            return RedirectToAction("Index", "ReciboNominas", new { id });
        }

        public async Task<IActionResult> PagoNomina()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            // cargar formulario
            var modelo = await _repoPagosEmitidos.FormRegistrarPagoNomina(idCondominio);
            // usar el repositorio?

            return View(modelo);
        }
        public IActionResult ConfirmarPago(PagoNominaVM modelo)
        {
            return View(modelo);
        }

        public IActionResult Comprobante(PagoNominaVM modelo)
        {
            return View(modelo);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RegistrarPagosPost(PagoNominaVM modelo)
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
                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = "Ya existe un pago registrado con este número de referencia!"
                            };

                            return View("Error", modeloError);
                        }
                    }

                    var resultado = await _repoPagosEmitidos.RegistrarPagoNomina(modelo);

                    if (resultado == "exito")
                    {
                        var condominio = await _context.Condominios.FindAsync(modelo.IdCondominio);

                        var idSubCuenta = (from c in _context.CodigoCuentasGlobals
                                           where c.IdCodCuenta == modelo.IdSubcuenta
                                           select c.IdSubCuenta).First();

                        var gasto = from c in _context.SubCuenta
                                    where c.Id == idSubCuenta
                                    select c;

                        var empleado = await _context.Empleados.FindAsync(modelo.IdEmpleado);
                        var deducciones = new List<Deduccion>();
                        var percepciones = new List<Percepcion>();
                        
                        if (empleado == null)
                        {
                            return NotFound();
                        }

                        if (modelo.percepciones && !modelo.deducciones)
                        {
                            percepciones = await _context.Percepciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToListAsync();
                        }
                        else if (!modelo.percepciones && modelo.deducciones)
                        {
                            deducciones = await _context.Deducciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToListAsync();
                        }
                        else if (modelo.percepciones && modelo.deducciones)
                        {
                            deducciones = await _context.Deducciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToListAsync();
                            percepciones = await _context.Percepciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToListAsync();
                        }                       

                        var comprobante = new ComprobantePagoNomina()
                        {
                            Condominio = condominio,
                            Concepto = modelo.Concepto,
                            Pagoforma = modelo.Pagoforma,
                            Mensaje = "¡Gracias por su pago!",
                            Gasto = gasto.First(),
                            Percepciones = percepciones,
                            Deducciones = deducciones,
                            Empleado = empleado
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

                        // validar deducciones y percepciones

                        if (modelo.percepciones && !modelo.deducciones)
                        {
                            comprobante.Pago.Monto = modelo.Monto + percepciones.Sum(c => c.Monto);

                        } else if (!modelo.percepciones && modelo.deducciones)
                        {
                            comprobante.Pago.Monto = modelo.Monto - deducciones.Sum(c => c.Monto);

                        }
                        else if (modelo.percepciones && modelo.deducciones)
                        {
                            comprobante.Pago.Monto = modelo.Monto + percepciones.Sum(c => c.Monto) - deducciones.Sum(c => c.Monto);

                        }
                        else
                        {
                            comprobante.Pago.Monto = modelo.Monto;
                        }
                        
                        comprobante.Pago.Fecha = modelo.Fecha;
                        TempData.Keep();

                        return View("Comprobante", comprobante);
                    }

                    ViewBag.FormaPago = "fallido";
                    ViewBag.Mensaje = resultado;
                    //traer subcuentas del condominio
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    modelo = await _repoPagosEmitidos.FormRegistrarPagoNomina(idCondominio);

                    TempData.Keep();

                    return View("PagoNomina", modelo);

                }
                //traer subcuentas del condominio
                var id = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                modelo = await _repoPagosEmitidos.FormRegistrarPagoNomina(id);

                TempData.Keep();

                ViewBag.FormaPago = "fallido";
                ViewBag.Mensaje = "Ha ocurrido un error inesperado";

                return View("PagoNomina", modelo);

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
        public ContentResult ComprobantePDF([FromBody] ComprobantePagoNomina modelo)
        {
            try
            {
                var data = _servicesPDF.ComprobantePagosNominaPDF(modelo);
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
