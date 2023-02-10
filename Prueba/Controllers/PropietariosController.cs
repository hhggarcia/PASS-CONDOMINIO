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
        private readonly PruebaContext _context;

        public PropietariosController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            IRelacionGastoRepository repoRelacionGasto,
            IReportesRepository repoReportes,
            PruebaContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _repoRelacionGasto = repoRelacionGasto;
            _repoReportes = repoReportes;
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
                        var listaPagosPorPropiedad = await _context.PagoRecibidos.Where(c => c.IdPropiedad == item.IdPropiedad).ToListAsync();
                        modelo.Add(item, listaPagosPorPropiedad);
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
                    var inmuebles = await _context.Inmuebles.Where(i => i.IdInmueble == propiedades.First().IdInmueble).ToListAsync();
                    var condominios = await _context.Condominios.Where(i => i.IdCondominio == inmuebles.First().IdCondominio).ToListAsync();
                    var modelo = await _repoReportes.InformacionGeneral(condominios.First().IdCondominio);
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
                var cuentas = from c in _context.Cuenta
                              select c;

                var subcuentas = from c in _context.SubCuenta
                                 select c;

                //CARGAR SELECT DE SUB CUENTAS DE BANCOS
                var bancos = from c in _context.Cuenta
                             where c.Descripcion.ToUpper().Trim() == "BANCO"
                             select c;

                var caja = from c in _context.Cuenta
                           where c.Descripcion.ToUpper().Trim() == "CAJA"
                           select c;

                var subcuentasBancos = from c in _context.SubCuenta
                                       where c.IdCuenta == bancos.First().Id
                                       select c;

                var subcuentasCaja = from c in _context.SubCuenta
                                     where c.IdCuenta == caja.First().Id
                                     select c;

                var propiedad = await _context.Propiedads.FindAsync(valor);

                var recibos = from c in _context.ReciboCobros
                              where c.IdPropiedad == valor
                              select c;

                modelo.Saldo = propiedad.Saldo;

                modelo.Deuda = propiedad.Deuda;

                modelo.Recibos = await recibos.Where(c => c.EnProceso == false && c.Pagado == false).ToListAsync();

                if (modelo.Recibos.Any())
                {
                    modelo.RecibosModel = await recibos.Where(c => c.EnProceso == false && c.Pagado == false)
                        .Select(c => new SelectListItem { Text = c.Fecha.ToString("dd/MM/yyyy"), Value = c.IdReciboCobro.ToString() })
                        .ToListAsync();
                    modelo.SubCuentasBancos = await subcuentasBancos.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() })
                        .ToListAsync();
                    modelo.SubCuentasCaja = await subcuentasCaja.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() })
                        .ToListAsync();

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
            if (ModelState.IsValid)
            {
                try
                {
                    var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
                    if (propiedad != null)
                    {
                        var inmueble = await _context.Inmuebles.FindAsync(propiedad.IdInmueble);
                        var condominio = await _context.Condominios.FindAsync(inmueble.IdCondominio);
                        var recibo = await _context.ReciboCobros.FindAsync(modelo.IdRecibo);
                        //var referenciaDolar = await _context.ReferenciaDolars.Where(c => c.Fecha.Date == DateTime.Today.Date).ToListAsync();

                        //if (!referenciaDolar.Any())
                        //{
                        //    referenciaDolar = await _context.ReferenciaDolars.ToListAsync();
                        //}
                        var comprobante = new ComprobanteVM()
                        {
                            Propiedad = propiedad,
                            Inmueble = inmueble,
                            Condominio = condominio,
                            FechaComprobante = DateTime.Today

                        };

                        if (modelo.Pagoforma == FormaPago.Transferencia)
                        {
                            var pago = new PagoRecibido()
                            {
                                IdPropiedad = modelo.IdPropiedad,
                                Fecha = modelo.Fecha,
                                IdSubCuenta = modelo.IdCodigoCuentaBanco,
                                Concepto = modelo.Concepto,
                                Confirmado = false,
                            };

                            pago.FormaPago = true;

                            var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);
                            if (recibo != null)
                            {
                                var monto = recibo.Monto;

                                if (monto > 0 && (propiedad.Saldo > 0 || propiedad.Deuda > 0))
                                {
                                    pago.Monto = monto;
                                }
                                else
                                {
                                    comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                    return View("Comprobante", comprobante);
                                }

                                // regitrar en bd el pago
                                using (var dbcontext = new PruebaContext())
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
                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia);
                                    dbcontext.SaveChanges();
                                }

                                // cambiar a -en proceso = true- el recibo mas actual 
                                var reciboActual = recibo;
                                reciboActual.EnProceso = true;

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReciboCobros.Update(reciboActual);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia;
                                comprobante.Mensaje = "Gracias por su pago!";

                            }

                            return View("Comprobante", comprobante);

                        }
                        else
                        {
                            var pago = new PagoRecibido()
                            {
                                IdPropiedad = modelo.IdPropiedad,
                                Fecha = modelo.Fecha,
                                IdSubCuenta = modelo.IdCodigoCuentaCaja,
                                Concepto = modelo.Concepto
                            };

                            pago.FormaPago = false;

                            if (recibo != null)
                            {
                                var monto = recibo.Monto;

                                if (monto > 0)
                                {
                                    pago.Monto = monto;
                                }
                                else
                                {
                                    comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                    return View("Comprobante", comprobante);
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }
                                // cambiar a -en proceso = true- el recibo mas actual 
                                var reciboActual = recibo;
                                reciboActual.EnProceso = true;

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReciboCobros.Update(reciboActual);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Mensaje = "Gracias por su pago!";
                            }
                            

                            return View("Comprobante", comprobante);
                        }
                    }
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

            return RedirectToAction("RegistrarPagos");
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

            var relacionGasto = await _context.RelacionGastos
                .Include(r => r.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdRgastos == id);
            if (relacionGasto == null)
            {
                return NotFound();
            }
            var modelo = await _repoRelacionGasto.LoadDataRelacionGastosMes(id);
            //return View(relacionGasto);
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
                    var inmuebles = await _context.Inmuebles.Where(i => i.IdInmueble == propiedades.First().IdInmueble).ToListAsync();
                    var condominios = await _context.Condominios.Where(i => i.IdCondominio == inmuebles.First().IdCondominio).ToListAsync();
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
    }
}
