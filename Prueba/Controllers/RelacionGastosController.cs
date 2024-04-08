using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class RelacionGastosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IRelacionGastoRepository _repoRelacionGastos;
        private readonly IMonedaRepository _repoMoneda;
        private readonly IPDFServices _servicePDF;
        private readonly NuevaAppContext _context;

        public RelacionGastosController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IRelacionGastoRepository repoRelacionGastos,
            IMonedaRepository repoMoneda,
            IPDFServices PDFService,
            NuevaAppContext context)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _repoRelacionGastos = repoRelacionGastos;
            _repoMoneda = repoMoneda;
            _servicePDF = PDFService;
        }

        // GET: RelacionGastos
        public async Task<IActionResult> Index()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var NuevaAppContext = _context.RelacionGastos.Where(r => r.IdCondominio == idCondominio);

            TempData.Keep();

            return View(await NuevaAppContext.ToListAsync());
        }

        /// <summary>
        /// Muestra los gastos, fondos y porvisiones en el mes actual
        /// al intentar crear nueva Relación de Gastos. Si ya existe 
        /// una relación para el mes actual, no se creará.
        /// </summary>
        /// <returns>Vista del detalle de la Relación de Gastos actual</returns>
        [HttpGet]
        public async Task<IActionResult> RelaciondeGastos()
        {
            try
            {
                // CARGAR GASTOS REGISTRADOS

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoRelacionGastos.LoadDataRelacionGastos(idCondominio);

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> TransaccionesDelMes()
        {
            try
            {
                // CARGAR GASTOS REGISTRADOS

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoRelacionGastos.LoadTransacciones(idCondominio);

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
        /// <summary>
        /// Muestra los detalles de gastos, fondos y provisiones
        /// de una Relación de Gastos específica
        /// </summary>
        /// <param name="id">Id de la Relación de Gastos seleccionada</param>
        /// <returns></returns>
        // GET: RelacionGastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RelacionGastos == null)
            {
                return NotFound();
            }

            var relacionGasto = await _context.RelacionGastos.Include(r => r.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdRgastos == id);
            if (relacionGasto == null)
            {
                return NotFound();
            }

            var modelo = await _repoRelacionGastos.LoadTransaccionesMes(id);
            //modelo.IdDetail = id;
            //return View(relacionGasto);
            return View(modelo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: RelacionGastos/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            return View();
        }

        // POST: RelacionGastos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relacionGasto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRgastos,SubTotal,TotalMensual,Fecha,IdCondominio")] RelacionGasto relacionGasto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(relacionGasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", relacionGasto.IdCondominio);
            return View(relacionGasto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: RelacionGastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RelacionGastos == null)
            {
                return NotFound();
            }

            var relacionGasto = await _context.RelacionGastos.FindAsync(id);
            if (relacionGasto == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", relacionGasto.IdCondominio);
            return View(relacionGasto);
        }

        // POST: RelacionGastos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="relacionGasto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRgastos,SubTotal,TotalMensual,Fecha,IdCondominio")] RelacionGasto relacionGasto)
        {
            if (id != relacionGasto.IdRgastos)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                _context.Update(relacionGasto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repoRelacionGastos.RelacionGastoExists(relacionGasto.IdRgastos))
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
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", relacionGasto.IdCondominio);
            //ViewData["IdDolar"] = new SelectList(_context.Condominios, "IdDolar", "Valor", relacionGasto.IdDolar);
            //return View(relacionGasto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: RelacionGastos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RelacionGastos == null)
            {
                return NotFound();
            }

            var relacionGasto = await _context.RelacionGastos.Include(r => r.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdRgastos == id);
            if (relacionGasto == null)
            {
                return NotFound();
            }

            return View(relacionGasto);
        }


        /// <summary>
        /// Confirma la elimnación de una RG 
        /// </summary>
        /// <param name="id">Id de la Relación de Gastos</param>
        /// <returns></returns>
        // POST: RelacionGastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RelacionGastos == null)
            {
                return Problem("Entity set 'NuevaAppContext.RelacionGastos'  is null.");
            }
            var relacionGasto = await _context.RelacionGastos.FindAsync(id);
            if (relacionGasto != null)
            {
                var result = await _repoRelacionGastos.DeleteRecibosCobroRG(id);
                var transaccions = await _context.RelacionGastoTransaccions.Where(c => c.IdRelacionGasto == id).ToListAsync();

                _context.RelacionGastoTransaccions.RemoveRange(transaccions);

                if (result)
                {
                    _context.RelacionGastos.Remove(relacionGasto);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Sino existe una Relación de Gastos,
        /// generará un Recibo de Cobro por cada Propiedad
        /// si la propiedad está solvente, se cambiará a deudor
        /// </summary>
        /// <returns>Index de Relaciones de Gastos</returns> 
        //public async Task<IActionResult> GenerarReciboCobro()
        //{
        //    try
        //    {
        //        int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

        //        var relacionesDeGasto = from c in _context.RelacionGastos
        //                                where c.IdCondominio == idCondominio
        //                                select c;

        //        var existeActualRG = await relacionesDeGasto.Where(c => c.Fecha.Month == DateTime.Today.Month).ToListAsync();

        //        if (!existeActualRG.Any())
        //        {
        //            var modelo = await _repoRelacionGastos.LoadDataRelacionGastos(idCondominio);
        //            // CARGAR PROPIEDADES DE CADA INMUEBLE DEL CONDOMINIO
        //            //var inmueblesCondominio = from c in _context.Inmuebles
        //            //                          where c.IdCondominio == idCondominio
        //            //                          select c;

        //            var propiedades = from c in _context.Propiedads
        //                              select c;
        //            var propietarios = from c in _context.AspNetUsers
        //                               select c;

        //            // Moneda Principal para hacer la referencia de Monto respecto al dolar
        //            var monedaPrincipal = await _repoMoneda.MonedaPrincipal(idCondominio);

        //            // REGISTRAR EN BD LA RELACION DE GASTOS
        //            var relacionGasto = new RelacionGasto
        //            {
        //                SubTotal = modelo.SubTotal,
        //                TotalMensual = modelo.Total,
        //                Fecha = DateTime.Today,
        //                IdCondominio = idCondominio,
        //                MontoRef = modelo.Total / monedaPrincipal.First().ValorDolar,
        //                ValorDolar = monedaPrincipal.First().ValorDolar,
        //                SimboloMoneda = monedaPrincipal.First().Simbolo,
        //                SimboloRef = "$"
        //            };

        //            using (var db_context = new NuevaAppContext())
        //            {
        //                await db_context.AddAsync(relacionGasto);
        //                await db_context.SaveChangesAsync();
        //            }

        //            IList<Propiedad> listaPropiedadesCondominio = new List<Propiedad>();
        //            if (propiedades != null && propiedades.Any())
        //            {
        //                var propiedadsCond = await propiedades.Where(c => c.IdCondominio == idCondominio).ToListAsync();
        //                var aux2 = listaPropiedadesCondominio.Concat(propiedadsCond).ToList();
        //                listaPropiedadesCondominio = aux2;

        //                // CALCULAR LOS PAGOS DE CADA PROPIEDAD POR SU ALICUOTA
        //                // CAMBIAR SOLVENCIA, SALDO Y DEUDA DE LA PROPIEDAD
        //                IList<ReciboCobro> recibosCobroCond = new List<ReciboCobro>();
        //                //IList<RelacionGastosEmailVM> recibosCobroEmail = new List<RelacionGastosEmailVM>();
        //                foreach (var propiedad in listaPropiedadesCondominio)
        //                {
        //                    // BUSCAR PUESTOS DE ESTACIONAMIENTO EXTRAS
        //                    //var puestos = await _context.PuestoEs.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
        //                    // VERIFICAR SOLVENCIA
        //                    // SI ES TRUE (ESTA SOLVENTE, AL DIA) NO SE BUSCA EN LA DEUDA
        //                    // SI ES FALSO PARA EL MONTO TOTAL A PAGAR DEBE MOSTRARSELE LA DEUDA
        //                    // Y EL TOTAL DEL MES MAS LA DEUDA
        //                    if (propiedad.Solvencia)
        //                    {
        //                        propiedad.Saldo += relacionGasto.TotalMensual * propiedad.Alicuota / 100;
        //                        propiedad.Solvencia = false;

        //                    }
        //                    else
        //                    {
        //                        propiedad.Deuda += propiedad.Saldo;
        //                        propiedad.Saldo += relacionGasto.TotalMensual * propiedad.Alicuota / 100;
        //                    }
        //                    // INFO DEL RECIBO

        //                    var recibo = new ReciboCobro();


        //                    recibo.IdPropiedad = propiedad.IdPropiedad;
        //                    recibo.IdRgastos = relacionGasto.IdRgastos;
        //                    recibo.Monto = relacionGasto.TotalMensual * propiedad.Alicuota / 100;
        //                    recibo.Fecha = DateTime.Today;
        //                    recibo.Abonado = 0;
        //                    recibo.MontoRef = (relacionGasto.TotalMensual * (propiedad.Alicuota) / 100) / monedaPrincipal.First().ValorDolar;
        //                    recibo.ValorDolar = monedaPrincipal.First().ValorDolar;
        //                    recibo.SimboloMoneda = monedaPrincipal.First().Simbolo;
        //                    recibo.SimboloRef = "$";

        //                    recibosCobroCond.Add(recibo);
        //                }

        //                // REGISTRAR EN BD LOS RECIBOS DE COBRO PARA CADA PROPIEDAD
        //                //  Y EDITAR LAS PROPIEDADES

        //                using (var db_context = new NuevaAppContext())
        //                {
        //                    foreach (var item in recibosCobroCond)
        //                    {
        //                        await db_context.AddAsync(item);
        //                    }
        //                    foreach (var propiedad in listaPropiedadesCondominio)
        //                    {
        //                        db_context.Update(propiedad);
        //                    }
        //                    await db_context.SaveChangesAsync();
        //                }

        //                // CREAR MODELO PARA NUEVA VISTA
        //                var aux = new RecibosCreadosVM
        //                {
        //                    Propiedades = listaPropiedadesCondominio,
        //                    Propietarios = propietarios.ToList(),
        //                    Recibos = recibosCobroCond,
        //                    //Inmuebles = inmueblesCondominio.ToList(),
        //                    RelacionGastos = modelo
        //                };

        //                // REENVIAR A OTRA VISTA

        //                ViewBag.Recibos = "Exitoso";

        //                TempData.Keep();

        //                return RedirectToAction("Index");
        //            }
        //        }
        //        else
        //        {
        //            TempData.Keep();

        //            var modeloError = new ErrorViewModel()
        //            {
        //                RequestId = "Ya existe una Relación de Gastos para este mes!"
        //            };

        //            return View("Error", modeloError);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Recibos = "Fallido";
        //        ViewBag.MsgError = "Error: " + ex.Message;
        //        var aux = new RecibosCreadosVM();
        //        TempData.Keep();
        //        return View(aux);
        //    }

        //    return RedirectToAction("Index");

        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Index Realcion de Gastos</returns>
        public async Task<IActionResult> RecibosCobroTransacciones()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var relacionesDeGasto = await (from c in _context.RelacionGastos
                                               where c.IdCondominio == idCondominio
                                               where c.Fecha.Month == DateTime.Today.Month
                                               select c).ToListAsync();

                if (!relacionesDeGasto.Any())
                {
                    // buscar condominio                
                    var condominio = await _context.Condominios.FindAsync(idCondominio);

                    if (condominio != null)
                    {
                        // buscar transacciones
                        var transaccionesDelMes = await _repoRelacionGastos.LoadTransacciones(condominio.IdCondominio);
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(idCondominio);

                        // generar Relacion de Gastos con las transacciones
                        // y sus relaciones
                        var relacionGasto = new RelacionGasto
                        {
                            IdCondominio = idCondominio,
                            SubTotal = transaccionesDelMes.Total,
                            TotalMensual = transaccionesDelMes.Total,
                            Fecha = DateTime.Today,
                            MontoRef = transaccionesDelMes.Total / monedaPrincipal.First().ValorDolar,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = monedaPrincipal.First().Simbolo,
                            SimboloRef = "$"
                        };

                        _context.RelacionGastos.Add(relacionGasto);
                        var idRelacionGatos = await _context.SaveChangesAsync();

                        if (idRelacionGatos != 0)
                        {
                            foreach (var transaccion in transaccionesDelMes.Transaccions)
                            {
                                var relacionTransaccion = new RelacionGastoTransaccion
                                {
                                    IdRelacionGasto = relacionGasto.IdRgastos,
                                    IdTransaccion = transaccion.IdTransaccion
                                };

                                _context.RelacionGastoTransaccions.Add(relacionTransaccion);
                                await _context.SaveChangesAsync();
                            }

                            IList<ReciboCobro> recibosCobroCond = new List<ReciboCobro>();
                            // buscar propiedades
                            var propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                            if (propiedades.Any())
                            {
                                // para cada propiedad

                                foreach (var propiedad in propiedades)
                                {
                                    decimal monto = 0;

                                    // --> ver sus grupos y alicuota
                                    var gruposPropiedad = await _context.PropiedadesGrupos
                                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad)
                                        .ToListAsync();
                                    
                                    // -----> recorrer transacciones si grupoCuenta == transaccionCuenta
                                    foreach (var transaccion in transaccionesDelMes.Transaccions)
                                    {
                                        // por cada transaccion 
                                        // revisar si esta entre los grupos de la propiedad

                                        if (gruposPropiedad.Exists(c => c.IdGrupoGasto == transaccion.IdGrupo))
                                        {
                                            var grupo = gruposPropiedad.FirstOrDefault(c => c.IdGrupoGasto == transaccion.IdGrupo);

                                            monto += transaccion.MontoTotal * (grupo.Alicuota / 100);
                                        }
                                    }

                                    // revisar transacciones individuales
                                    var individuales = transaccionesDelMes.TransaccionesIndividuales.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();
                                    if (individuales.Any())
                                    {
                                        monto += individuales.Sum(c => c.MontoTotal);
                                    }

                                    // VALIDAR DEUDAS PARA ACTUALIZAR  PROPIEDAD
                                    if (propiedad.Solvencia)
                                    {
                                        propiedad.Saldo = monto;
                                        propiedad.Solvencia = false;
                                    }
                                    else
                                    {
                                        propiedad.Deuda += propiedad.Saldo;
                                        propiedad.Saldo = monto;

                                        // buscar recibos anteriores no pagados
                                        var recibosAnt = await _context.ReciboCobros
                                            .Where(c => c.IdPropiedad == propiedad.IdPropiedad && c.Pagado == false && c.EnProceso == false)
                                            .ToListAsync();

                                        // mora para cada recibo y sumar a la propiedad
                                        // indexacion por cada recibo y sumar a la propiedad

                                        var mora = recibosAnt.Sum(c => c.MontoMora);
                                        var indexacion = recibosAnt.Sum(c => c.MontoIndexacion);

                                        propiedad.MontoIntereses += mora;
                                        propiedad.MontoMulta += indexacion;
                                    }

                                    // generar recibos
                                    var recibo = new ReciboCobro
                                    {
                                        IdPropiedad = propiedad.IdPropiedad,
                                        IdRgastos = relacionGasto.IdRgastos,
                                        Monto = monto,
                                        Fecha = DateTime.Today,
                                        Pagado = false,
                                        EnProceso = false,
                                        Abonado = 0,
                                        MontoRef = monto / monedaPrincipal.First().ValorDolar,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = monedaPrincipal.First().Simbolo,
                                        SimboloRef = "$",
                                        MontoMora = monto * (condominio.InteresMora / 100),
                                        MontoIndexacion = monto * ((decimal)condominio.Multa / 100)
                                    };

                                    recibosCobroCond.Add(recibo);
                                    // registrar recibo 
                                    // actualizar propiedad

                                    _context.ReciboCobros.Add(recibo);
                                    _context.Propiedads.Update(propiedad);
                                }
                            }
                            await _context.SaveChangesAsync();

                            // CREAR MODELO PARA NUEVA VISTA
                            var aux = new RecibosCreadosVM
                            {
                                Propiedades = propiedades,
                                Recibos = recibosCobroCond,
                                RelacionGastosTransacciones = transaccionesDelMes,
                                RelacionGasto = relacionGasto
                            };

                            return View("RecibosCreados", aux);

                        }

                        return RedirectToAction("Index");
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData.Keep();

                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una Relación de Gastos para este mes!"
                    };

                    return View("Error", modeloError);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Recibos = "Fallido";
                ViewBag.MsgError = "Error: " + ex.Message;
                var aux = new RecibosCreadosVM();
                TempData.Keep();
                return View(aux);
            }
        }
        public IActionResult RecibosCreados()
        {
            var aux = new RecibosCreadosVM();
            return View(aux);
        }
        [HttpGet]
        public async Task<IActionResult> ReciboPdf()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var dataCondominio = await _context.Condominios.Where(c => c.IdCondominio == idCondominio).FirstAsync();
            var modelo = await _repoRelacionGastos.LoadDataRelacionGastos(idCondominio);

            var data = _servicePDF.RelacionGastosPDF(modelo);
            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "Recibo.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> RelacionGastosPDF(int id)
        {
            var modelo = await _repoRelacionGastos.LoadDataRelacionGastosMes(id);
            var data = _servicePDF.RelacionGastosPDF(modelo);
            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "Recibo.pdf");
        }

        //[HttpPost]
        //public ContentResult RelacionGastosPDF([FromBody] RelacionDeGastosVM relacionDeGastos)
        //{
        //    try
        //    {
        //        var data = _servicePDF.RelacionGastosPDF(relacionDeGastos);
        //        var base64 = Convert.ToBase64String(data);
        //        return Content(base64, "application/pdf");

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Error generando PDF: {e.Message}");
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
        //    }
        //}
    }
}
