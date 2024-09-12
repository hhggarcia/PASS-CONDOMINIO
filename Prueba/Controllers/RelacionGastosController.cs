using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
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
        private readonly IEmailService _emailService;
        private readonly NuevaAppContext _context;

        public RelacionGastosController(IEmailService emailService,
            IUnitOfWork unitOfWork,
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
            _emailService = emailService;
        }

        // GET: RelacionGastos
        public async Task<IActionResult> Index()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var NuevaAppContext = _context.RelacionGastos
                .Where(r => r.IdCondominio == idCondominio)
                .OrderBy(c => c.Fecha);

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
        /// <param name="id">id del recibo a buscar el detalle</param>
        /// <returns></returns>
        public async Task<IActionResult> DetalleReciboTransacciones(int id)
        {
            var recibo = await _context.ReciboCobros.FindAsync(id);
            var modelo = new DetalleReciboTransaccionesVM();
            if (recibo != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);

                modelo.Recibo = recibo;
                modelo.Propiedad = propiedad;
                modelo.GruposPropiedad = gruposPropiedad;
                modelo.RelacionGasto = rg;
                modelo.Transacciones = transacciones;
            }

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

                if (result)
                {
                    _context.RelacionGastos.Remove(relacionGasto);
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
        /// Creacion de los recibos de cobro
        /// </summary>
        /// <returns>Index Realcion de Gastos</returns>
        public async Task<IActionResult> RecibosCobroTransacciones()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var relacionesDeGasto = await (from c in _context.RelacionGastos
                                               where c.IdCondominio == idCondominio
                                               where DateTime.Compare(c.Fecha, DateTime.Today) == 0
                                               //where DateTime.Today.Month == c.Fecha.Month
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
                        var mes = (DateTime.Today.Month - 1).ToString() + "-" + DateTime.Today.AddMonths(-1).ToString("MMM").ToUpper() + "-" + DateTime.Today.ToString("yyyy");
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
                            SimboloRef = "$",
                            Mes = mes
                        };

                        _context.RelacionGastos.Add(relacionGasto);
                        var idRelacionGatos = await _context.SaveChangesAsync();

                        if (idRelacionGatos != 0)
                        {
                            foreach (var transaccion in transaccionesDelMes.Transaccions)
                            {
                                transaccion.Activo = false;

                                var relacionTransaccion = new RelacionGastoTransaccion
                                {
                                    IdRelacionGasto = relacionGasto.IdRgastos,
                                    IdTransaccion = transaccion.IdTransaccion
                                };

                                _context.RelacionGastoTransaccions.Add(relacionTransaccion);
                                _context.Transaccions.Update(transaccion);
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
                                        if (transaccion.IdPropiedad == null && gruposPropiedad.Exists(c => c.IdGrupoGasto == transaccion.IdGrupo))
                                        {
                                            if (!transaccion.TipoTransaccion)
                                            {
                                                var grupo = gruposPropiedad.First(c => c.IdGrupoGasto == transaccion.IdGrupo);
                                                if (grupo != null)
                                                {
                                                    monto += transaccion.MontoTotal * (grupo.Alicuota / 100);
                                                }
                                            }
                                            else
                                            {
                                                var grupo = gruposPropiedad.First(c => c.IdGrupoGasto == transaccion.IdGrupo);
                                                if (grupo != null)
                                                {
                                                    monto -= transaccion.MontoTotal * (grupo.Alicuota / 100);
                                                }
                                            }
                                        }
                                    }

                                    // buscar fondosd 

                                    // aplicar con la alicuota de la propiedad
                                    // BUSCAR FONDOS
                                    var fondos = from f in _context.Fondos
                                                 join c in _context.CodigoCuentasGlobals
                                                 on f.IdCodCuenta equals c.IdCodCuenta
                                                 where c.IdCondominio == condominio.IdCondominio
                                                 where DateTime.Compare(DateTime.Today, f.FechaFin) < 0 && DateTime.Compare(DateTime.Today, f.FechaInicio) >= 0
                                                 select f;

                                    foreach (var fondo in fondos)
                                    {
                                        if (fondo.Porcentaje != null && fondo.Porcentaje > 0)
                                        {
                                            monto += (transaccionesDelMes.Total * (decimal)fondo.Porcentaje / 100) * (propiedad.Alicuota / 100);
                                        }

                                        if (fondo.Monto != null && fondo.Monto > 0)
                                        {
                                            monto += (decimal)fondo.Monto * (propiedad.Alicuota / 100);
                                        }
                                    }

                                    // revisar transacciones individuales
                                    var individuales = transaccionesDelMes.TransaccionesIndividuales.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();
                                    if (individuales.Any())
                                    {
                                        monto += individuales.Sum(c => c.MontoTotal);

                                        foreach (var item in individuales)
                                        {
                                            item.Activo = false;
                                            var relacionTransaccionInd = new RelacionGastoTransaccion
                                            {
                                                IdRelacionGasto = relacionGasto.IdRgastos,
                                                IdTransaccion = item.IdTransaccion
                                            };

                                            _context.Transaccions.Update(item);
                                            _context.RelacionGastoTransaccions.Add(relacionTransaccionInd);

                                        }
                                    }
                                    var credito = propiedad.Creditos != null ? (decimal)propiedad.Creditos : 0;

                                    // VALIDAR DEUDAS PARA ACTUALIZAR  PROPIEDAD
                                    var reciboVencido = await _context.ReciboCobros
                                            .FirstOrDefaultAsync(c => c.IdPropiedad == propiedad.IdPropiedad && c.ReciboActual);


                                    /// AGREGAR LAS MODIFICACIONES 
                                    /// A LAS PROPIEDADES CON Solvencia = false
                                    /// si la propiedad esta solvente solo necesita
                                    /// modificar saldo = x | 

                                    // mora para cada recibo y sumar a la propiedad
                                    // indexacion por cada recibo y sumar a la propiedad
                                    decimal mora = 0;
                                    decimal indexacion = 0;

                                    if (reciboVencido != null)
                                    {
                                        reciboVencido.ReciboActual = false;
                                        _context.ReciboCobros.Update(reciboVencido);
                                    }

                                    if (propiedad.Solvencia)
                                    {
                                        propiedad.Saldo = monto - credito;
                                        propiedad.Creditos = 0;
                                        propiedad.Solvencia = false;                                       
                                    }
                                    else
                                    {
                                        propiedad.Saldo = monto - credito;
                                        propiedad.Creditos = 0;
                                        propiedad.Solvencia = false;

                                        if (reciboVencido != null && !reciboVencido.Pagado)
                                        {
                                            if (reciboVencido.Abonado > 0 && reciboVencido.Abonado < reciboVencido.Monto)
                                            {
                                                mora = (reciboVencido.Monto - reciboVencido.Abonado) * condominio.InteresMora / 100;
                                                indexacion = (reciboVencido.Monto - reciboVencido.Abonado) * (decimal)condominio.Multa / 100;

                                            }
                                            else if (reciboVencido.Abonado == 0)
                                            {
                                                mora = reciboVencido.MontoMora;
                                                indexacion = reciboVencido.MontoIndexacion;
                                            }

                                            propiedad.Deuda += propiedad.Saldo;

                                            reciboVencido.TotalPagar = reciboVencido.Monto + mora + indexacion - reciboVencido.Abonado;
                                            reciboVencido.TotalPagar = reciboVencido.TotalPagar < 0 ? 0 : reciboVencido.TotalPagar;                                            
                                        }
                                    }
                                    

                                    // generar recibo nuevo
                                    var recibo = new ReciboCobro
                                    {
                                        IdPropiedad = propiedad.IdPropiedad,
                                        IdRgastos = relacionGasto.IdRgastos,
                                        Monto = monto - credito,
                                        Fecha = DateTime.Today,
                                        Pagado = false,
                                        EnProceso = false,
                                        Abonado = 0,
                                        MontoRef = monto / monedaPrincipal.First().ValorDolar,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = monedaPrincipal.First().Simbolo,
                                        SimboloRef = "$",
                                        MontoMora = monto * (condominio.InteresMora / 100),
                                        MontoIndexacion = monto * ((decimal)condominio.Multa / 100),
                                        Mes = mes,
                                        ReciboActual = true,
                                        TotalPagar = 0
                                    };

                                    recibosCobroCond.Add(recibo);
                                    // registrar recibo 
                                    // actualizar propiedad

                                    _context.ReciboCobros.Add(recibo);
                                    _context.Propiedads.Update(propiedad);
                                    await _context.SaveChangesAsync();

                                }

                                // CREAR MODELO PARA NUEVA VISTA
                                var aux = new RecibosCreadosVM
                                {
                                    Propiedades = propiedades,
                                    Recibos = recibosCobroCond,
                                    RelacionGastosTransacciones = transaccionesDelMes,
                                    RelacionGasto = relacionGasto
                                };
                                TempData.Keep();

                                return View("RecibosCreados", aux);
                            }

                            TempData.Keep();

                            var modeloError3 = new ErrorViewModel()
                            {
                                RequestId = "Error al buscar propiedades!"
                            };

                            return View("Error", modeloError3);
                        }

                        TempData.Keep();

                        var modeloError2 = new ErrorViewModel()
                        {
                            RequestId = "Ya existe una Relación de Gastos para este mes!"
                        };

                        return View("Error", modeloError2);

                    }
                    TempData.Keep();

                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Error buscando su condominio!"
                    };

                    return View("Error", modeloError);
                }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                TempData.Keep();

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Error: " + ex.Message
                };

                return View("Error", modeloError);
            }
        }
        public async Task<IActionResult> PreRecibos()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                // buscar condominio                
                var condominio = await _context.Condominios.FindAsync(idCondominio);

                if (condominio != null)
                {
                    // buscar transacciones
                    var transaccionesDelMes = await _repoRelacionGastos.LoadTransacciones(condominio.IdCondominio);
                    var monedaPrincipal = await _repoMoneda.MonedaPrincipal(idCondominio);
                    var mes = (DateTime.Today.Month - 1).ToString() + "-" + DateTime.Today.AddMonths(-1).ToString("MMM").ToUpper() + "-" + DateTime.Today.ToString("yyyy");
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
                        SimboloRef = "$",
                        Mes = mes
                    };

                    //_context.RelacionGastos.Add(relacionGasto);
                    var idRelacionGatos = 1;

                    if (idRelacionGatos != 0)
                    {
                        //foreach (var transaccion in transaccionesDelMes.Transaccions)
                        //{
                        //    transaccion.Activo = false;

                        //    var relacionTransaccion = new RelacionGastoTransaccion
                        //    {
                        //        IdRelacionGasto = relacionGasto.IdRgastos,
                        //        IdTransaccion = transaccion.IdTransaccion
                        //    };

                        //    //_context.RelacionGastoTransaccions.Add(relacionTransaccion);
                        //    //_context.Transaccions.Update(transaccion);
                        //    //await _context.SaveChangesAsync();
                        //}

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
                                    if (transaccion.IdPropiedad == null && gruposPropiedad.Exists(c => c.IdGrupoGasto == transaccion.IdGrupo))
                                    {
                                        if (!transaccion.TipoTransaccion)
                                        {
                                            var grupo = gruposPropiedad.First(c => c.IdGrupoGasto == transaccion.IdGrupo);

                                            monto += transaccion.MontoTotal * (grupo.Alicuota / 100);
                                        }
                                        else
                                        {
                                            var grupo = gruposPropiedad.First(c => c.IdGrupoGasto == transaccion.IdGrupo);

                                            monto -= transaccion.MontoTotal * (grupo.Alicuota / 100);
                                        }
                                    }
                                }

                                // buscar fondosd 

                                // aplicar con la alicuota de la propiedad
                                // BUSCAR FONDOS
                                var fondos = from f in _context.Fondos
                                             join c in _context.CodigoCuentasGlobals
                                             on f.IdCodCuenta equals c.IdCodCuenta
                                             where c.IdCondominio == condominio.IdCondominio
                                             where DateTime.Compare(DateTime.Today, f.FechaFin) < 0 && DateTime.Compare(DateTime.Today, f.FechaInicio) >= 0
                                             select f;

                                if (fondos != null && fondos.Any())
                                {
                                    foreach (var fondo in fondos)
                                    {
                                        if (fondo.Porcentaje != null && fondo.Porcentaje > 0)
                                        {
                                            monto += (transaccionesDelMes.Total * (decimal)fondo.Porcentaje / 100) * (propiedad.Alicuota / 100);
                                        }

                                        if (fondo.Monto != null && fondo.Monto > 0)
                                        {
                                            monto += (decimal)fondo.Monto * (propiedad.Alicuota / 100);
                                        }
                                    }
                                }


                                // revisar transacciones individuales

                                if (transaccionesDelMes.TransaccionesIndividuales != null && transaccionesDelMes.TransaccionesIndividuales.Any())
                                {
                                    var individuales = transaccionesDelMes.TransaccionesIndividuales.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();

                                    monto += individuales.Sum(c => c.MontoTotal);

                                    foreach (var item in individuales)
                                    {
                                        item.Activo = false;
                                        // _context.Transaccions.Update(item);
                                    }
                                }
                                var credito = propiedad.Creditos != null ? (decimal)propiedad.Creditos : 0;
                                // VALIDAR DEUDAS PARA ACTUALIZAR  PROPIEDAD
                                if (propiedad.Solvencia)
                                {
                                    propiedad.Saldo = monto - credito;
                                    propiedad.Creditos = 0;
                                    propiedad.Solvencia = false;
                                }
                                else
                                {
                                    propiedad.Deuda += propiedad.Saldo;
                                    propiedad.Saldo = monto - credito;
                                    propiedad.Creditos = 0;
                                    // buscar recibos anteriores no pagados
                                    //var recibosAnt = await _context.ReciboCobros
                                    //    .Where(c => c.IdPropiedad == propiedad.IdPropiedad && !c.Pagado)
                                    //    .ToListAsync();

                                    var reciboVencido = await _context.ReciboCobros
                                        .FirstOrDefaultAsync(c => c.IdPropiedad == propiedad.IdPropiedad && !c.Pagado && c.ReciboActual);

                                    // mora para cada recibo y sumar a la propiedad
                                    // indexacion por cada recibo y sumar a la propiedad
                                    decimal mora = 0;
                                    decimal indexacion = 0;

                                    //if (recibosAnt.Any())
                                    //{
                                    //    mora = recibosAnt.Sum(c => c.MontoMora);
                                    //    indexacion = recibosAnt.Sum(c => c.MontoIndexacion);
                                    //}

                                    if (reciboVencido != null)
                                    {
                                        mora = reciboVencido.MontoMora;
                                        indexacion = reciboVencido.MontoIndexacion;

                                        reciboVencido.ReciboActual = false;

                                        //_context.ReciboCobros.Update(reciboVencido);
                                    }

                                    propiedad.MontoIntereses += mora;
                                    propiedad.MontoMulta += indexacion;
                                }

                                // generar recibos
                                var recibo = new ReciboCobro
                                {
                                    IdPropiedad = propiedad.IdPropiedad,
                                    IdRgastos = relacionGasto.IdRgastos,
                                    Monto = monto - credito,
                                    Fecha = DateTime.Today,
                                    Pagado = false,
                                    EnProceso = false,
                                    Abonado = 0,
                                    MontoRef = monto / monedaPrincipal.First().ValorDolar,
                                    ValorDolar = monedaPrincipal.First().ValorDolar,
                                    SimboloMoneda = monedaPrincipal.First().Simbolo,
                                    SimboloRef = "$",
                                    MontoMora = monto * (condominio.InteresMora / 100),
                                    MontoIndexacion = monto * ((decimal)condominio.Multa / 100),
                                    Mes = mes,
                                    ReciboActual = true
                                };

                                recibosCobroCond.Add(recibo);
                                // registrar recibo 
                                // actualizar propiedad

                                //_context.ReciboCobros.Add(recibo);
                                //_context.Propiedads.Update(propiedad);
                            }
                        }
                        //await _context.SaveChangesAsync();

                        // CREAR MODELO PARA NUEVA VISTA
                        var aux = new RecibosCreadosVM
                        {
                            Propiedades = propiedades,
                            Recibos = recibosCobroCond,
                            RelacionGastosTransacciones = transaccionesDelMes,
                            RelacionGasto = relacionGasto
                        };

                        //return View("RecibosCreados", aux);
                        var modelo = new List<DetalleReciboTransaccionesVM>();
                        foreach (var recibo in aux.Recibos)
                        {
                            var item = new DetalleReciboTransaccionesVM();
                            if (recibo != null)
                            {
                                var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                                //var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                                var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                                //var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);

                                item.Recibo = recibo;
                                item.Propiedad = propiedad;
                                item.GruposPropiedad = gruposPropiedad;
                                item.RelacionGasto = aux.RelacionGasto;
                                item.Transacciones = aux.RelacionGastosTransacciones;
                            }

                            modelo.Add(item);
                        }

                        var data = _servicePDF.TodosRecibosTransaccionesPDF(modelo);
                        Stream stream = new MemoryStream(data);
                        return File(stream, "application/pdf", "RecibosCondominio.pdf");

                    }

                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index");
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

        /// <summary>
        /// Muestra los recibos de cada propiedad 
        /// de una relacion de gastos especificos
        /// </summary>
        /// <param name="id">Id relacion de gastos</param>
        /// <returns></returns>
        public async Task<IActionResult> VerRecibos(int id)
        {
            var rg = await _context.RelacionGastos.FindAsync(id);
            var modelo = new RecibosCreadosVM();

            if (rg != null)
            {
                var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);
                var propiedades = await _context.Propiedads.Where(c => c.IdCondominio == rg.IdCondominio).ToListAsync();
                var recibos = await _context.ReciboCobros.Where(c => c.IdRgastos == rg.IdRgastos).ToListAsync();

                modelo.RelacionGasto = rg;
                modelo.RelacionGastosTransacciones = transacciones;
                modelo.Recibos = recibos;
                modelo.Propiedades = propiedades;
            }

            return View("RecibosCreados", modelo);
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
            var rg = await _context.RelacionGastos.FindAsync(id);
            if (rg != null)
            {
                var modelo = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);
                var data = _servicePDF.Transacciones(modelo);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "RelacionGasto.pdf");
            }

            return RedirectToAction("Index");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id del recibo a consultar</param>
        /// <returns></returns>
        public async Task<IActionResult> DetalleReciboTransaccionesPDF(int id)
        {
            var recibo = await _context.ReciboCobros.FindAsync(id);
            var modelo = new DetalleReciboTransaccionesVM();
            if (recibo != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);

                modelo.Recibo = recibo;
                modelo.Propiedad = propiedad;
                modelo.GruposPropiedad = gruposPropiedad;
                modelo.RelacionGasto = rg;
                modelo.Transacciones = transacciones;

                var data = await _servicePDF.DetalleReciboTransaccionesPDF(modelo);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "Recibo_" + propiedad.Codigo + "_" + recibo.Fecha.ToString("dd/MM/yyyy") + ".pdf");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id de la relacion de gastos</param>
        /// <returns></returns>
        public async Task<IActionResult> TodosRecibosTransaccionesPDF(int id)
        {
            var modelo = new List<DetalleReciboTransaccionesVM>();

            var rg = await _context.RelacionGastos.FindAsync(id);
            if (rg != null)
            {
                var recibos = await _context.ReciboCobros.Where(c => c.IdRgastos == rg.IdRgastos).ToListAsync();

                foreach (var recibo in recibos)
                {
                    var item = new DetalleReciboTransaccionesVM();
                    if (recibo != null)
                    {
                        var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                        //var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                        var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                        var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);

                        item.Recibo = recibo;
                        item.Propiedad = propiedad;
                        item.GruposPropiedad = gruposPropiedad;
                        item.RelacionGasto = rg;
                        item.Transacciones = transacciones;
                    }

                    modelo.Add(item);
                }

                var data = _servicePDF.TodosRecibosTransaccionesPDF(modelo);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "RecibosCondominio.pdf");
            }
            return RedirectToAction("Index");
        }

        public IActionResult DataEmail(int id)
        {
            TempData["IdReciboCobro"] = id.ToString();

            TempData.Keep();

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"> Id de realcion de gastos</param>
        /// <returns></returns>
        public IActionResult DataEmailPropiedades(int id)
        {
            TempData["IdRG"] = id.ToString();

            TempData.Keep();

            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ConfirmSendEmailRecibo(EmailAttachmentPdf email)
        {
            int idRecibo = Convert.ToInt32(TempData.Peek("IdReciboCobro").ToString());

            var recibo = await _context.ReciboCobros.FindAsync(idRecibo);
            var modelo = new DetalleReciboTransaccionesVM();
            if (recibo != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);
                var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);

                modelo.Recibo = recibo;
                modelo.Propiedad = propiedad;
                modelo.GruposPropiedad = gruposPropiedad;
                modelo.RelacionGasto = rg;
                modelo.Transacciones = transacciones;

                var data = await _servicePDF.DetalleReciboTransaccionesPDF(modelo);

                email.From = modelo.Transacciones.Condominio.Email;
                email.To = usuario.Email;
                email.Pdf = data;
                email.FileName = "Recibo" + "_" + recibo.Fecha.ToString("dd/MM/yyyy") + propiedad.Codigo.ToString();

                var result = _emailService.SendEmailRG(email);

                if (!result.Contains("OK"))
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = result
                    };

                    return View("Error", modeloError);
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmSendEmailTodosRecibos(EmailAttachmentPdf email)
        {
            int idRg = Convert.ToInt32(TempData.Peek("IdRG").ToString());

            var rg = await _context.RelacionGastos.FindAsync(idRg);

            if (rg != null)
            {
                var recibos = await _context.ReciboCobros.Where(c => c.IdRgastos == rg.IdRgastos).ToListAsync();

                if (recibos.Any())
                {
                    foreach (var recibo in recibos)
                    {
                        //var recibo = await _context.ReciboCobros.FindAsync(idRecibo);
                        var modelo = new DetalleReciboTransaccionesVM();

                        var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                        //var rg = await _context.RelacionGastos.FindAsync(recibo.IdRgastos);
                        var gruposPropiedad = await _context.PropiedadesGrupos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                        var transacciones = await _repoRelacionGastos.LoadTransaccionesMes(rg.IdRgastos);
                        var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);

                        modelo.Recibo = recibo;
                        modelo.Propiedad = propiedad;
                        modelo.GruposPropiedad = gruposPropiedad;
                        modelo.RelacionGasto = rg;
                        modelo.Transacciones = transacciones;

                        var data = await _servicePDF.DetalleReciboTransaccionesPDF(modelo);

                        email.From = modelo.Transacciones.Condominio.Email;
                        email.To = usuario.Email;
                        email.Pdf = data;
                        email.FileName = "Recibo" + "_" + recibo.Fecha.ToString("dd/MM/yyyy") + propiedad.Codigo.ToString();

                        var result = _emailService.SendEmailRG(email);

                    }
                }
            }

            return RedirectToAction("Index");

        }
    }
}
