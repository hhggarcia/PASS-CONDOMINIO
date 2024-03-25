using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Models;
using Prueba.Services;
using Prueba.Utils;
using Prueba.Repositories;
using Prueba.ViewModels;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using System.Web.Razor.Parser.SyntaxTree;
using NetTopologySuite.Index.HPRtree;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Net;
using System.Reflection.Metadata;
using System.Linq;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequirePropietario")]

    public class PropietariosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailService _serviceEmail;
        private readonly IManageExcel _manageExcel;
        private readonly IRelacionGastoRepository _repoRelacionGasto;
        private readonly IReportesRepository _repoReportes;
        private readonly IMonedaRepository _repoMoneda;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly NuevaAppContext _context;
        private readonly IPDFServices _servicePDF;

        public PropietariosController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            IRelacionGastoRepository repoRelacionGasto,
            IReportesRepository repoReportes,
            IMonedaRepository repoMoneda,
            ICuentasContablesRepository repoCuentas,
            IPDFServices PDFService,
            NuevaAppContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _repoRelacionGasto = repoRelacionGasto;
            _repoReportes = repoReportes;
            _repoMoneda = repoMoneda;
            _repoCuentas = repoCuentas;
            _servicePDF = PDFService;
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Historial()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                //llamar a pagos realizados
                // pasar diccionario de propiedades y por cada propiedad una lista de pagos realizados
                var modelo = new Dictionary<Propiedad, List<PagoRecibido>>();
                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();
                if (propiedades != null && propiedades.Count() > 0)
                {
                    foreach (var item in propiedades)
                    {
                        var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();

                        var pagos = await (from p in _context.PagoRecibidos
                                           join cc in _context.PagosRecibos
                                           on p.IdPagoRecibido equals cc.IdPago
                                           join r in recibos
                                           on cc.IdRecibo equals r.IdReciboCobro
                                           select p).ToListAsync();

                        modelo.Add(item, pagos);
                    }
                }

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
        public async Task<IActionResult> DashboardUsuario()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();

                if (propiedades != null && propiedades.Any())
                {
                    //var inmuebles = await _context.Inmuebles.Where(i => i.IdInmueble == propiedades.First().IdInmueble).ToListAsync();
                    var condominios = await _context.Condominios.Where(i => i.IdCondominio == propiedades.First().IdCondominio).ToListAsync();
                    var modelo = await _repoReportes.InformacionGeneral(condominios.First().IdCondominio);

                    TempData["idCondominio"] = condominios.First().IdCondominio.ToString();
                    return View(modelo);
                }

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Este usuario no tiene propiedades"
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RegistrarPagos()
        {
            try
            {
                var modelo = new PagoRecibidoVM();

                string idPropietario = TempData.Peek("idUserLog").ToString();

                var propiedades = from c in _context.Propiedads
                                  where c.IdUsuario == idPropietario
                                  select c;

                modelo.Propiedades = await propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToListAsync();

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
        /// <param name="valor"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AjaxCargarRecibos(int valor)
        {
            PagoRecibidoVM modelo = new PagoRecibidoVM();

            if (valor > 0)
            {
                var propiedad = await _context.Propiedads.FindAsync(valor);

                if (propiedad != null)
                {
                    //var inmueble = await _context.Inmuebles.FindAsync(propiedad.IdInmueble);
                    var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);

                    var cuentasCondominio = from c in _context.CodigoCuentasGlobals
                                            where c.IdCondominio == condominio.IdCondominio
                                            select c;

                    var cuentas = from c in _context.Cuenta
                                  select c;

                    var subcuentas = from c in _context.SubCuenta
                                     select c;

                    //CARGAR SELECT DE SUB CUENTAS DE BANCOS
                    var subcuentasBancos = await _repoCuentas.ObtenerBancos(condominio.IdCondominio);
                    var subcuentasCaja = await _repoCuentas.ObtenerCaja(condominio.IdCondominio);


                    var recibos = from c in _context.ReciboCobros
                                  where c.IdPropiedad == valor
                                  select c;

                    modelo.Saldo = propiedad.Saldo;

                    modelo.Deuda = propiedad.Deuda;


                    //modelo.Recibos = await recibos.ToListAsync();
                    modelo.Recibos = await recibos.Where(c => (c.EnProceso == false && c.Pagado == false) || (c.EnProceso == true && c.Pagado == false)).ToListAsync();
                    //modelo.Recibos = await _context.ReciboCobros.FindAsync(valor);
                    modelo.Abonado = modelo.Recibos[0].Abonado;

                    if (modelo.Recibos.Any())
                    {
                        modelo.RecibosModel = await recibos.Where(c => c.EnProceso == false && c.Pagado == false)
                            .Select(c => new SelectListItem { Text = c.Fecha.ToString("dd/MM/yyyy"), Value = c.IdReciboCobro.ToString() })
                            .ToListAsync();
                        modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() })
                            .ToList();
                        modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() })
                            .ToList();

                    }
                }



            }

            return Json(modelo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Comprobante()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegistrarPagos(PagoRecibidoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {
                    //*Inicio a como estaba antes*-------------------------------------------------------
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    var idRecibo = _context.ReciboCobros
                          .Where(c => c.IdPropiedad == modelo.IdPropiedad)
                          .Select(c => c.IdReciboCobro)
                           .FirstOrDefault();
                    //var ejemplo = await _context.Propiedads.FindAsync()
                    var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);

                    if (propiedad != null)
                    {
                        // validar num referencia repetido
                        decimal montoReferencia = 0;
                        //var inmueble = await _context.Inmuebles.FindAsync(propiedad.IdInmueble);
                        var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);

                        var recibo = await _context.ReciboCobros.FindAsync(idRecibo);

                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(propiedad.IdCondominio);

                        var comprobante = new ComprobanteVM()
                        {
                            Propiedad = propiedad,
                            // Inmueble = inmueble,
                            Condominio = condominio,
                            FechaComprobante = DateTime.Today
                        };

                        if (modelo.Pagoforma == FormaPago.Transferencia)
                        {
                            var existPagoTransferencia = from p in _context.PagoRecibidos
                                                         join referencia in _context.ReferenciasPrs
                                                         on p.IdPagoRecibido equals referencia.IdPagoRecibido
                                                         where referencia.NumReferencia == modelo.NumReferencia
                                                         select new { p, referencia };

                            if (existPagoTransferencia != null && existPagoTransferencia.Any())
                            {
                                var modeloError = new ErrorViewModel()
                                {
                                    RequestId = "Ya existe un pago registrado con este número de Referencia!"
                                };

                                return View("Error", modeloError);
                            }

                            var idBanco = (from c in _context.CodigoCuentasGlobals
                                           where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                           select c).First();

                            // buscar moneda asigna a la subcuenta
                            var moneda = from m in _context.MonedaConds
                                         join mc in _context.MonedaCuenta
                                         on m.IdMonedaCond equals mc.IdMoneda
                                         where mc.IdCodCuenta == idBanco.IdCodCuenta
                                         select m;

                            var pago = new PagoRecibido()
                            {
                                //IdPropiedad = modelo.IdPropiedad,
                                IdCondominio = idCondominio,
                                Fecha = modelo.Fecha,
                                IdSubCuenta = idBanco.IdCodCuenta,
                                Concepto = modelo.Concepto,
                                Confirmado = false,
                            };

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                montoReferencia = modelo.Monto;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                var montoDolares = modelo.Monto * moneda.First().ValorDolar;

                                montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                                //montoReferencia = montoDolares;
                            }

                            pago.FormaPago = true;

                            var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);
                            if (recibo != null)
                            {

                                var monto = recibo.Monto;

                                if (monto > 0 && (propiedad.Saldo > 0 || propiedad.Deuda > 0))
                                {
                                    pago.Monto = montoReferencia;
                                    if (pago.Monto < recibo.Monto)
                                    {
                                        comprobante.Restante = recibo.Monto - (recibo.Abonado + pago.Monto);
                                    }
                                    else if (pago.Monto > recibo.Monto)
                                    {
                                        comprobante.Abonado = pago.Monto - recibo.Monto;
                                    }
                                    pago.SimboloMoneda = moneda.First().Simbolo;
                                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                                    pago.MontoRef = montoReferencia;
                                    pago.SimboloRef = monedaPrincipal.First().Simbolo;

                                }
                                else
                                {
                                    comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                    return View("Comprobante", comprobante);
                                }

                                // regitrar en bd el pago
                                using (var dbcontext = new NuevaAppContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                var referencia = new ReferenciasPr()
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    NumReferencia = modelo.NumReferencia,
                                    Banco = banco.Descricion
                                };

                                // regitrar en bd la referencia del pago
                                using (var dbcontext = new NuevaAppContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia);
                                    dbcontext.SaveChanges();
                                }

                                // cambiar a -en proceso = true- el recibo mas actual 
                                var reciboActual = recibo;
                                reciboActual.EnProceso = true;
                                reciboActual.Abonado += montoReferencia;

                                using (var dbcontext = new NuevaAppContext())
                                {
                                    dbcontext.ReciboCobros.Update(reciboActual);
                                    dbcontext.SaveChanges();
                                }

                                using (var dbcontext = new NuevaAppContext())
                                {
                                    var relacion = new PagosRecibo
                                    {
                                        IdPago = pago.IdPagoRecibido,
                                        IdRecibo = reciboActual.IdReciboCobro
                                    };

                                    dbcontext.Add(relacion);
                                    await dbcontext.SaveChangesAsync();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia;
                                comprobante.Mensaje = "Gracias por su pago!";

                            }

                            TempData.Keep();

                            return View("Comprobante", comprobante);

                        }
                        else
                        {
                            var idCaja = (from c in _context.CodigoCuentasGlobals
                                          where c.IdSubCuenta == modelo.IdCodigoCuentaCaja
                                          select c).First();

                            // buscar moneda asigna a la subcuenta
                            var moneda = from m in _context.MonedaConds
                                         join mc in _context.MonedaCuenta
                                         on m.IdMonedaCond equals mc.IdMoneda
                                         where mc.IdCodCuenta == idCaja.IdCodCuenta
                                         select m;

                            var pago = new PagoRecibido()
                            {
                                //IdPropiedad = modelo.IdPropiedad,
                                Fecha = modelo.Fecha,
                                IdSubCuenta = idCaja.IdCodCuenta,
                                Concepto = modelo.Concepto
                            };

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                montoReferencia = modelo.Monto;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                var montoDolares = modelo.Monto * moneda.First().ValorDolar;

                                montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                                //montoReferencia = montoDolares;
                            }

                            pago.FormaPago = false;

                            if (recibo != null)
                            {
                                var monto = recibo.Monto;

                                if (monto > 0)
                                {
                                    pago.Monto = montoReferencia;
                                    if (pago.Monto < recibo.Monto)
                                    {
                                        comprobante.Restante = recibo.Monto - (recibo.Abonado + pago.Monto);
                                    }
                                    else
                                    {
                                        comprobante.Abonado = pago.Monto - recibo.Monto;
                                    }
                                    pago.SimboloMoneda = moneda.First().Simbolo;
                                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                                    pago.MontoRef = montoReferencia;
                                    pago.SimboloRef = monedaPrincipal.First().Simbolo;
                                }
                                else
                                {
                                    comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                    return View("Comprobante", comprobante);
                                }

                                using (var dbcontext = new NuevaAppContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }
                                // cambiar a -en proceso = true- el recibo mas actual 
                                var reciboActual = recibo;
                                reciboActual.EnProceso = true;
                                reciboActual.Abonado += montoReferencia;

                                using (var dbcontext = new NuevaAppContext())
                                {
                                    dbcontext.ReciboCobros.Update(reciboActual);
                                    dbcontext.SaveChanges();
                                }
                                using (var dbcontext = new NuevaAppContext())
                                {
                                    var relacion = new PagosRecibo
                                    {
                                        IdPago = pago.IdPagoRecibido,
                                        IdRecibo = reciboActual.IdReciboCobro
                                    };

                                    dbcontext.Add(relacion);
                                    await dbcontext.SaveChangesAsync();
                                }
                                comprobante.PagoRecibido = pago;
                                comprobante.Mensaje = "Gracias por su pago!";
                            }

                            TempData.Keep();

                            return View("Comprobante", comprobante);
                        }
                    }
                }

                TempData.Keep();

                ViewBag.FormaPago = "fallido";

                //return RedirectToAction("RegistrarPagos");

                string idPropietario = TempData.Peek("idUserLog").ToString();

                var propiedades = from c in _context.Propiedads
                                  where c.IdUsuario == idPropietario
                                  select c;

                modelo.Propiedades = await propiedades.Select(c => new SelectListItem(c.Codigo, c.IdPropiedad.ToString())).ToListAsync();

                return View("RegistrarPagos", modelo);
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
        /// <returns></returns>
        public async Task<IActionResult> Recibos()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                var modelo = new Dictionary<Propiedad, List<ReciboCobro>>();
                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();
                if (propiedades != null && propiedades.Count() > 0)
                {
                    foreach (var item in propiedades)
                    {
                        var listaRecibosPorPropiedad = await _context.ReciboCobros.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();
                        modelo.Add(item, listaRecibosPorPropiedad);
                    }
                }

                TempData.Keep();

                return View(modelo);
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
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> DetalleRecibo(int id)
        {
            if (_context.RelacionGastos == null)
            {
                return NotFound();
            }
            //var relacionGasto = await _context.RelacionGastos
            //    .Include(r => r.IdCondominioNavigation)
            //    .FirstOrDefaultAsync(m => m.IdRgastos == id);
            //if (relacionGasto == null)
            //{
            //    return NotFound();
            //}
            var modelo = await _repoRelacionGasto.DetalleRecibo(id);
            var listaCuotas = await _context.CuotasEspeciales.Where(c => c.IdCondominio == modelo.Recibo.IdRgastosNavigation.IdCondominio).ToListAsync();
            var listaRecibos = await _context.ReciboCuotas.Where(c => c.IdPropiedad == modelo.Propiedad.IdPropiedad).ToListAsync();

            foreach (var cuotas in listaCuotas)
            {
                CuotasRecibosCobrosVM cuotasRecibosCobros = new CuotasRecibosCobrosVM()
                {
                    CuotasEspeciale = cuotas,
                };
                foreach (var recibos in listaRecibos)
                {
                    if (recibos.IdPropiedad == modelo.Propiedad.IdPropiedad)
                    {
                        cuotasRecibosCobros.ReciboCuota = recibos;
                    }
                }
                modelo.CuotasRecibosCobros.Add(cuotasRecibosCobros);
            }
            modelo.IdDetalleRecibo = id;
            return View(modelo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Deudores()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();

                if (propiedades != null && propiedades.Any())
                {
                    //var inmuebles = await _context.Inmuebles.Where(i => i.IdInmueble == propiedades.First().IdInmueble).ToListAsync();
                    var condominios = await _context.Condominios.Where(i => i.IdCondominio == propiedades.First().IdCondominio).ToListAsync();
                    //var modelo = await _repoReportes.InformacionGeneral(condominios.First().IdCondominio);
                    int idCondominio = condominios.First().IdCondominio;

                    var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

                    TempData.Keep();

                    return View(modelo);
                }
                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Este Usuario no tiene Propiedades en Condominios"
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

        [HttpGet]
        public async Task<IActionResult> ReciboPdf()
        {
            string idPropietario = TempData.Peek("idUserLog").ToString();

            var modelo = new Dictionary<Propiedad, List<ReciboCobro>>();
            var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();
            if (propiedades != null && propiedades.Count() > 0)
            {
                foreach (var item in propiedades)
                {
                    var listaRecibosPorPropiedad = await _context.ReciboCobros.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();
                    modelo.Add(item, listaRecibosPorPropiedad);
                }
            }
            var data = _servicePDF.ReciboPDF(modelo);
            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "Recibo.pdf");
        }
        [HttpGet]
        public async Task<IActionResult> HistorialPdf()
        {
            string idPropietario = TempData.Peek("idUserLog").ToString();

            var modelo = new Dictionary<Propiedad, List<PagoRecibido>>();
            var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).ToListAsync();
            if (propiedades != null && propiedades.Count() > 0)
            {
                foreach (var item in propiedades)
                {
                    //var listaPagosPorPropiedad = await _context.PagoRecibidos.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();
                    var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();

                    var pagos = await (from p in _context.PagoRecibidos
                                       join cc in _context.PagosRecibos
                                       on p.IdPagoRecibido equals cc.IdPago
                                       join r in recibos
                                       on cc.IdRecibo equals r.IdReciboCobro
                                       select p).ToListAsync();
                    modelo.Add(item, pagos);
                }
            }
            var data = _servicePDF.HistorialPDF(modelo);
            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "Historial.pdf");
        }
        [HttpGet]
        async public Task<IActionResult> DetallePdf(int id)
        {

            var modelo = await _repoRelacionGasto.DetalleRecibo(id);
            var listaCuotas = await _context.CuotasEspeciales.Where(c => c.IdCondominio == modelo.Recibo.IdRgastosNavigation.IdCondominio).ToListAsync();
            var listaRecibos = await _context.ReciboCuotas.Where(c => c.IdPropiedad == modelo.Propiedad.IdPropiedad).ToListAsync();
            var condominio = await _context.Condominios.Where(c => c.IdCondominio == modelo.Recibo.IdRgastosNavigation.IdCondominio).FirstAsync();

            foreach (var cuotas in listaCuotas)
            {
                CuotasRecibosCobrosVM cuotasRecibosCobros = new CuotasRecibosCobrosVM()
                {
                    CuotasEspeciale = cuotas,
                };
                foreach (var recibos in listaRecibos)
                {
                    if (recibos.IdPropiedad == modelo.Propiedad.IdPropiedad)
                    {
                        cuotasRecibosCobros.ReciboCuota = recibos;
                    }
                }
                modelo.CuotasRecibosCobros.Add(cuotasRecibosCobros);
            }
            modelo.condominio = condominio;

            var data = _servicePDF.DetalleReciboPDF(modelo);
            var base64 = Convert.ToBase64String(data);
            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "DetalleRecibo.pdf");

        }
        [HttpPost]
        public ContentResult DetalleReciboPdf([FromBody] DetalleReciboVM detalleReciboVM)
        {
            try
            {
                var data = _servicePDF.DetalleReciboPDF(detalleReciboVM);
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
        public ContentResult ComprobantePDF([FromBody] ComprobanteVM comprobanteVM)
        {
            try
            {
                var data = _servicePDF.ComprobantePDF(comprobanteVM);
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
        public async Task<IActionResult> PagarCuotaEspeciales(int? id)
        {
            try
            {
                var reciboCuota = await _context.ReciboCuotas.Where(c => c.IdReciboCuotas == id).FirstAsync();

                var condominio = await _context.CuotasEspeciales
                    .Where(c => c.IdCuotaEspecial == reciboCuota.IdCuotaEspecial)
                    .Select(c => c.IdCondominio)
                    .FirstAsync();

                //CARGAR SELECT DE SUB CUENTAS DE BANCOS
                var subcuentasBancos = await _repoCuentas.ObtenerBancos((int)condominio);
                var subcuentasCaja = await _repoCuentas.ObtenerCaja((int)condominio);

                PagoRecibidoCuotaVM pagoRecibidoCuotaVM = new PagoRecibidoCuotaVM
                {
                    idRecibo = reciboCuota.IdReciboCuotas,
                    ReciboCuotas = reciboCuota,
                    SubCuentasBancos = (IList<SelectListItem>)subcuentasBancos.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() }).ToList(),
                    SubCuentasCaja = (IList<SelectListItem>)subcuentasCaja.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() }).ToList()
                };

                TempData.Keep();
                return View(pagoRecibidoCuotaVM);

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
        public async Task<IActionResult> PagarCuotaEspeciales(PagoRecibidoCuotaVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                    decimal montoReferencia = 0;
                    var idRecibo = modelo.idRecibo;
                    var reciboCuota = await _context.ReciboCuotas.Where(c => c.IdReciboCuotas == idRecibo).FirstOrDefaultAsync();
                    var cuotaEspecial = await _context.CuotasEspeciales.Where(c => c.IdCuotaEspecial == reciboCuota.IdCuotaEspecial).FirstOrDefaultAsync();
                    var propiedad = await _context.Propiedads.FindAsync(reciboCuota.IdPropiedad);
                    //var inmueble = await _context.Inmuebles.FindAsync(propiedad.IdInmueble);
                    var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);
                    var monedaPrincipal = await _repoMoneda.MonedaPrincipal(propiedad.IdCondominio);

                    var comprobante = new ComprobanteCEVM()
                    {
                        Propiedad = propiedad,
                        Condominio = condominio,
                        FechaComprobante = DateTime.Today,
                        CuotasEspeciale = cuotaEspecial
                    };
                    if (modelo.Pagoforma == FormaPago.Transferencia)
                    {
                        var existPagoTransferencia = from p in _context.PagoRecibidos
                                                     join referencia in _context.ReferenciasPrs
                                                     on p.IdPagoRecibido equals referencia.IdPagoRecibido
                                                     where referencia.NumReferencia == modelo.NumReferencia
                                                     select new { p, referencia };

                        if (existPagoTransferencia != null && existPagoTransferencia.Any())
                        {
                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = "Ya existe un pago registrado con este número de Referencia!"
                            };

                            return View("Error", modeloError);
                        }

                        var idBanco = (from c in _context.CodigoCuentasGlobals
                                       where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                       select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in _context.MonedaConds
                                     join mc in _context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idBanco.IdCodCuenta
                                     select m;

                        //var pago = new PagoReciboCuota()
                        //{
                        //    IdPropiedad = reciboCuota.IdPropiedad,
                        //    Fecha = modelo.Fecha,
                        //    IdSubCuenta = idBanco.IdCodCuenta,
                        //    Concepto = modelo.Concepto,
                        //    Confirmado = false,
                        //    IdCuota = reciboCuota.IdCuotaEspecial
                        //};
                        var pago = new PagoRecibido()
                        {
                            //IdPropiedad = reciboCuota.IdPropiedad,
                            IdCondominio = idCondominio,
                            Fecha = modelo.Fecha,
                            IdSubCuenta = idBanco.IdCodCuenta,
                            Confirmado = false,
                        };

                        if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = (decimal)modelo.Monto;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            var montoDolares = modelo.Monto * moneda.First().ValorDolar;

                            montoReferencia = (decimal)(montoDolares * monedaPrincipal.First().ValorDolar);
                        }

                        pago.FormaPago = true;

                        var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);
                        if (reciboCuota != null)
                        {

                            var monto = reciboCuota.SubCuotas;

                            if (monto > 0)
                            {
                                pago.Monto = montoReferencia;
                                if (pago.Monto < reciboCuota.SubCuotas)
                                {
                                    comprobante.Restante = (decimal)(reciboCuota.SubCuotas - (reciboCuota.Abonado + pago.Monto));
                                }
                                else if (pago.Monto > reciboCuota.SubCuotas)
                                {
                                    comprobante.Abonado = (decimal)(pago.Monto - reciboCuota.SubCuotas);
                                }
                                pago.SimboloMoneda = moneda.First().Simbolo;
                                pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                                pago.MontoRef = montoReferencia;
                                pago.SimboloRef = monedaPrincipal.First().Simbolo;
                            }
                            else
                            {
                                comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                return View("ComprobanteCE", comprobante);
                            }

                            // regitrar en bd el pago
                            using (var dbcontext = new NuevaAppContext())
                            {
                                dbcontext.PagoRecibidos.Add(pago);
                                dbcontext.SaveChanges();
                            }

                            var referencia = new ReferenciasPr()
                            {
                                IdPagoRecibido = pago.IdPagoRecibido,
                                NumReferencia = modelo.NumReferencia,
                                Banco = banco.Descricion
                            };

                            // regitrar en bd la referencia del pago
                            using (var dbcontext = new NuevaAppContext())
                            {
                                dbcontext.ReferenciasPrs.Add(referencia);
                                dbcontext.SaveChanges();
                            }

                            // cambiar a -en proceso = true- el recibo mas actual 
                            var reciboActual = reciboCuota;
                            reciboActual.EnProceso = true;
                            reciboActual.Abonado += montoReferencia;

                            using (var dbcontext = new NuevaAppContext())
                            {
                                dbcontext.ReciboCuotas.Update(reciboActual);
                                dbcontext.SaveChanges();
                            }

                            using (var dbcontext = new NuevaAppContext())
                            {
                                //var relacion = new PagosCuotasRecibido
                                //{
                                //    IdPago = pago.IdPagoRecibido,
                                //    IdRecibido = reciboActual.IdReciboCuotas,
                                //    IdCuotaEspecial = reciboActual.IdCuotaEspecial
                                //};
                                var relacion = new PagosCuotas
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    IdReciboCuota = reciboActual.IdReciboCuotas,
                                    //IdCuotaEspecial = reciboActual.IdCuotaEspecial
                                };
                                dbcontext.Add(relacion);
                                await dbcontext.SaveChangesAsync();
                            }

                            comprobante.PagoRecibido = pago;
                            comprobante.Referencias = referencia;
                            comprobante.Mensaje = "Gracias por su pago!";

                        }
                        TempData.Keep();

                        return View("ComprobanteCE", comprobante);

                    }
                    else
                    {
                        var idCaja = (from c in _context.CodigoCuentasGlobals
                                      where c.IdSubCuenta == modelo.IdCodigoCuentaCaja
                                      select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in _context.MonedaConds
                                     join mc in _context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idCaja.IdCodCuenta
                                     select m;

                        //var pago = new PagoReciboCuota()
                        //{
                        //    IdSubCuenta = idCaja.IdCodCuenta,
                        //    Concepto = modelo.Concepto,
                        //    IdPropiedad = reciboCuota.IdPropiedad,
                        //    Fecha = modelo.Fecha,
                        //    Confirmado = false,
                        //    IdCuota = reciboCuota.IdCuotaEspecial
                        //};
                        var pago = new PagoRecibido()
                        {
                            IdCondominio = idCondominio,
                            IdSubCuenta = idCaja.IdCodCuenta,
                            Concepto = modelo.Concepto,
                            //IdPropiedad = reciboCuota.IdPropiedad,
                            Fecha = modelo.Fecha,
                            Confirmado = false
                        };
                        if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = (decimal)modelo.Monto;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            var montoDolares = modelo.Monto * moneda.First().ValorDolar;

                            montoReferencia = (decimal)(montoDolares * monedaPrincipal.First().ValorDolar);
                            //montoReferencia = montoDolares;
                        }
                        pago.FormaPago = false;

                        if (reciboCuota != null)
                        {
                            var monto = reciboCuota.SubCuotas;

                            if (monto > 0)
                            {
                                pago.Monto = montoReferencia;
                                if (pago.Monto < reciboCuota.SubCuotas)
                                {
                                    comprobante.Restante = (decimal)(reciboCuota.SubCuotas - (reciboCuota.Abonado + pago.Monto));
                                }
                                else
                                {
                                    comprobante.Abonado = (decimal)(pago.Monto - reciboCuota.SubCuotas);
                                }
                                pago.SimboloMoneda = moneda.First().Simbolo;
                                pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                                pago.MontoRef = montoReferencia;
                                pago.SimboloRef = monedaPrincipal.First().Simbolo;
                            }
                            else
                            {
                                comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                TempData.Keep();

                                return View("ComprobanteCE", comprobante);
                            }

                            using (var dbcontext = new NuevaAppContext())
                            {
                                dbcontext.PagoRecibidos.Add(pago);
                                dbcontext.SaveChanges();
                            }
                            // cambiar a -en proceso = true- el recibo mas actual 
                            var reciboActual = reciboCuota;
                            reciboActual.EnProceso = true;
                            reciboActual.Abonado += montoReferencia;

                            using (var dbcontext = new NuevaAppContext())
                            {
                                dbcontext.ReciboCuotas.Update(reciboActual);
                                dbcontext.SaveChanges();
                            }
                            using (var dbcontext = new NuevaAppContext())
                            {
                                //var relacion = new PagosCuotasRecibido
                                //{
                                //    IdPago = pago.IdPagoRecibido,
                                //    IdRecibido = reciboActual.IdReciboCuotas,
                                //    IdCuotaEspecial = reciboActual.IdCuotaEspecial
                                //};
                                var relacion = new PagosCuotas
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    IdReciboCuota = reciboActual.IdReciboCuotas,
                                    //IdCuotaEspecial = reciboActual.IdCuotaEspecial
                                };

                                dbcontext.Add(relacion);
                                await dbcontext.SaveChangesAsync();
                            }
                            comprobante.PagoRecibido = pago;
                            comprobante.Mensaje = "Gracias por su pago!";
                        }
                        TempData.Keep();

                        return View("ComprobanteCE", comprobante);
                    }
                }
                else
                {
                    TempData.Keep();

                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Este usuario presenta problemas al pagar."
                    };

                    return View("Error", modeloError);
                }

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
        public async Task<IActionResult> ReciboCuotasEspeciales()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == idPropietario).Select(c => c.IdPropiedad).ToListAsync();

                if (idPropietario != null)
                {
                    var recibosConCodigoPropiedad = from recibo in _context.ReciboCuotas
                                                    join propiedad in _context.Propiedads
                                                    on recibo.IdPropiedad equals propiedad.IdPropiedad
                                                    where propiedades.Contains(recibo.IdPropiedad)
                                                    select new ReciboCuotaVM
                                                    {
                                                        Codigo = propiedad.Codigo,
                                                        ReciboCuota = recibo
                                                    };
                    List<ReciboCuotaVM> listaRecibosDelAdmin = recibosConCodigoPropiedad.ToList();
                    //var listaRecibosCuotas = await _context.ReciboCuotas.Where(c => propiedades.Contains(c.IdPropiedad)).ToListAsync();

                    TempData.Keep();

                    return View(listaRecibosDelAdmin);
                }

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Este Usuario no tiene Propiedades en Condominios"
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
        public async Task<IActionResult> DetalleReciboCuotasEspeciales(int? id)
        {
            if (id != null)
            {

                var modelo = await _context.ReciboCuotas.Where(c => c.IdReciboCuotas == id).FirstOrDefaultAsync();
                var cuotaEspecial = await _context.CuotasEspeciales.Where(c => c.IdCuotaEspecial == modelo.IdCuotaEspecial).FirstOrDefaultAsync();
                var detalleReciboCuotasVM = new DetalleReciboCuotasVM
                {
                    reciboCuota = modelo,
                    cuotasEspeciale = cuotaEspecial,

                };


                return View(detalleReciboCuotasVM);
            }
            else
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Este Usuario no tiene Propiedades en Condominios"
                };

                return View("Error", modeloError);
            }
        }

        [HttpPost]
        public ContentResult ComprobanteCEVMPDF([FromBody] ComprobanteCEVM comprobanteCEVM)
        {
            try
            {
                var data = _servicePDF.ComprobanteCEVMPDF(comprobanteCEVM);
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
