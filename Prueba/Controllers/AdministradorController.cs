using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.Utils;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class AdministradorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailService _serviceEmail;
        private readonly IManageExcel _manageExcel;
        private readonly IReportesRepository _repoReportes;
        private readonly PruebaContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        /// <param name="userStore"></param>
        /// <param name="serviceEmail"></param>
        /// <param name="manageExcel"></param>
        /// <param name="context"></param>
        public AdministradorController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            IReportesRepository repoReportes,
            PruebaContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _repoReportes = repoReportes;
            _context = context;
        }

        /// <summary>
        /// Busca los condominios asignados a un Administrador
        /// </summary>
        /// <returns>Vista con todos los condominios</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                //CON EL ID DEL USUARIO LOGEADO BUSCAR SUS CONDOMINIOS ASIGNADOS
                var idAdministrador = TempData.Peek("idUserLog").ToString();

                //CARGAR LIST DE CONDOMINIOS
                var condominios = from c in _context.Condominios
                                  where c.IdAdministrador == idAdministrador
                                  select c;

                foreach (var item in condominios)
                {
                    var inmuebles = from i in _context.Inmuebles
                                    where item.IdCondominio == i.IdCondominio
                                    select i;

                    item.Inmuebles = await inmuebles.ToListAsync();
                }

                var condominiosModel = await condominios.ToListAsync();

                TempData.Keep();

                return View(condominiosModel);

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
        /// <param name="id">id del condominio seleccionado</param>
        /// <returns>Vista del dashboard</returns>
        public async Task<IActionResult> Dashboard(int id)
        {
            try
            {
                // GUARDAR ID CONDOMINIO PARA SELECCIONAR SUS CUENTAS CONTABLES
                TempData["idCondominio"] = id.ToString();
                var modelo = await _repoReportes.InformacionGeneral(id);
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
        public async Task<IActionResult> Deudores()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await LoadDataDeudores(idCondominio);

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
        public async Task<IActionResult> PagosRecibidos()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                // propietarios
                IList<ApplicationUser> listaPropietarios = await _signInManager.UserManager.Users.ToListAsync();
                // propiedades
                var propiedadesPorUsuario = new Dictionary<ApplicationUser, List<Propiedad>>();
                // pagos recibidos
                var pagosPorPropiedad = new Dictionary<Propiedad, List<PagoRecibido>>();

                foreach (var user in listaPropietarios)
                {
                    var propiedades = await _context.Propiedads.Where(c => c.IdUsuario == user.Id).ToListAsync();
                    if (propiedades != null && propiedades.Count() > 0)
                    {
                        propiedadesPorUsuario.Add(user, propiedades);
                        foreach (var propiedad in propiedades)
                        {
                            var pagos = await _context.PagoRecibidos.Where(
                                c => c.IdPropiedad == propiedad.IdPropiedad
                                && c.Confirmado == false)
                                .ToListAsync();

                            if (pagos != null && pagos.Count() > 0)
                            {
                                pagosPorPropiedad.Add(propiedad, pagos);
                            }
                            continue;
                        }
                    }
                    continue;
                }
                IndexPagoRecibdioVM modelo = new IndexPagoRecibdioVM()
                {
                    UsuariosPropiedad = propiedadesPorUsuario,
                    PropiedadPagos = pagosPorPropiedad,
                    ReferenciasDolar = await _context.ReferenciaDolars.ToListAsync()
                };

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
        [HttpGet]
        public IActionResult RectificarPago(int id)
        {
            try
            {
                TempData["idPagoConfirmar"] = id.ToString();
                return View();
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
        public async Task<IActionResult> ConfirmarRectificarPago()
        {
            try
            {
                int id = Convert.ToInt32(TempData.Peek("idPagoConfirmar").ToString());

                // buscar pago
                var pago = await _context.PagoRecibidos.FindAsync(id);

                if (pago != null)
                {
                    var propiedad = await _context.Propiedads.FindAsync(pago.IdPropiedad);

                    if (propiedad != null)
                    {
                        // buscar recibos de la propiedad
                        var recibosPropiedad = await _context.ReciboCobros.Where(
                            c => c.IdPropiedad == propiedad.IdPropiedad
                            && c.EnProceso == true
                            && c.Pagado == false)
                            .ToListAsync();

                        var reciboActual = recibosPropiedad.Where(c => c.Monto == pago.Monto).ToList();


                        // buscar referencia si tiene y eliminar pago

                        if (pago.FormaPago)
                        {
                            var referencia = await _context.ReferenciasPrs.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                            _context.ReferenciasPrs.Remove(referencia.First());
                            _context.PagoRecibidos.Remove(pago);                            
                        }
                        else
                        {
                            _context.PagoRecibidos.Remove(pago);
                        }

                        reciboActual.First().EnProceso = false;

                        _context.ReciboCobros.Update(reciboActual.First());

                        await _context.SaveChangesAsync();
                    }

                }

                TempData.Keep();

                return RedirectToAction("PagosRecibidos");

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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ConfirmarPagoRecibido(int id)
        {
            try
            {
                TempData["idPagoConfirmar"] = id.ToString();
                return View();
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
        [HttpPost]
        public async Task<IActionResult> ConfirmarPagoRecibidoPost()
        {
            try
            {
                int id = Convert.ToInt32(TempData.Peek("idPagoConfirmar").ToString());

                // buscar pago
                var pago = await _context.PagoRecibidos.FindAsync(id);

                if (pago != null)
                {
                    // buscar propiedad
                    var propiedad = await _context.Propiedads.FindAsync(pago.IdPropiedad);

                    if (propiedad != null)
                    {
                        // buscar recibos de la propiedad
                        var recibosPropiedad = await _context.ReciboCobros.Where(
                            c => c.IdPropiedad == propiedad.IdPropiedad
                            && c.EnProceso == true
                            && c.Pagado == false)
                            .ToListAsync();

                        // buscar subcuenta contable donde esta el pago del condominio
                        var cuentaCondominio = await _context.SubCuenta.Where(c => c.IdCuenta == 15 && c.Codigo == "01").ToListAsync();

                        // buscar referencia si tiene
                        var referencias = new List<ReferenciasPr>();
                        var cuentaAfectada = new List<SubCuenta>();
                        if (pago.FormaPago)
                        {
                            referencias = await _context.ReferenciasPrs.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                            cuentaAfectada = await _context.SubCuenta.Where(c => c.Id == pago.IdSubCuenta).ToListAsync();
                        }
                        else
                        {
                            cuentaAfectada = await _context.SubCuenta.Where(c => c.Id == pago.IdSubCuenta).ToListAsync();
                        }

                        // buscar la manera de hacer match (por el monto ??)

                        //var reciboActual = recibosPropiedad.Where(c => c.Fecha.Month == DateTime.Now.Month).ToList();
                        var reciboActual = recibosPropiedad.Where(c => c.Monto == pago.Monto).ToList();
                        try
                        {
                            // se pago solo el recibo del mes actual
                            using (var dbContext = new PruebaContext())
                            {
                                if (propiedad.Saldo == reciboActual.First().Monto
                                    && propiedad.Deuda == 0)
                                {
                                    // hacer saldo = 0
                                    propiedad.Saldo = 0;
                                    // cambiar solvencia = True
                                    propiedad.Solvencia = true;

                                }
                                else if (propiedad.Saldo == 0
                                    && propiedad.Deuda >= reciboActual.First().Monto)
                                {
                                    // restar de Deuda -Monto
                                    propiedad.Deuda -= reciboActual.First().Monto;
                                    // si deuda y saldo == 0 -> solvencia = true
                                    if (propiedad.Deuda == 0)
                                    {
                                        propiedad.Solvencia = true;
                                    }
                                }
                                else if (propiedad.Saldo > 0
                                    && propiedad.Saldo != reciboActual.First().Monto
                                    && propiedad.Deuda >= reciboActual.First().Monto)
                                {
                                    // restar de Deuda -Monto
                                    propiedad.Deuda -= reciboActual.First().Monto;

                                }

                                // cambiar en recibo o en los recibos - en proceso a false y pagado a true
                                reciboActual.First().EnProceso = false;
                                reciboActual.First().Pagado = true;

                                // cambiar pago.Confirmado a True
                                pago.Confirmado = true;

                                dbContext.Update(propiedad);
                                dbContext.Update(reciboActual.First());
                                dbContext.Update(pago);

                                int numAsiento = 1;

                                if (dbContext.LdiarioGlobals.Count() > 0)
                                {
                                    numAsiento = dbContext.LdiarioGlobals.ToList().Last().NumAsiento;
                                }

                                // libro diario cuenta afecada
                                // asiento con los ingresos (Condominio) aumentar por haber Concepto (pago condominio -propiedad- -mes-)
                                // -> contra subcuenta  banco o caja por el debe
                                LdiarioGlobal asientoBanco = new LdiarioGlobal
                                {
                                    IdCodCuenta = cuentaAfectada.First().Id,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = true,
                                    NumAsiento = numAsiento + 1
                                };

                                LdiarioGlobal asientoIngreso = new LdiarioGlobal
                                {
                                    IdCodCuenta = cuentaCondominio.First().Id,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = false,
                                    NumAsiento = numAsiento + 1
                                };

                                dbContext.Add(asientoIngreso);
                                dbContext.Add(asientoBanco);

                                dbContext.SaveChanges();

                                // registrar asientos en bd

                                var ingreso = new Ingreso
                                {
                                    IdAsiento = asientoIngreso.IdAsiento,
                                };

                                var activo = new Activo
                                {
                                    IdAsiento = asientoBanco.IdAsiento,
                                };

                                using (var db_context = new PruebaContext())
                                {
                                    db_context.Add(ingreso);
                                    db_context.Add(activo);

                                    db_context.SaveChanges();
                                }

                            }
                            TempData.Keep();

                            return RedirectToAction("PagosRecibidos");
                        }
                        catch (Exception ex)
                        {
                            var error = new ErrorViewModel()
                            {
                                RequestId = ex.Message
                            };

                            return View("Error", error);
                        }
                    }

                }

                TempData.Keep();
                return RedirectToAction("PagosRecibidos");
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
        public async Task<IActionResult> EstadoResultado()
        {
            try
            {
                // buscar id de condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var subcuentas = from c in _context.SubCuenta
                                 select c;
                var clases = from c in _context.Clases
                             select c;
                var grupos = from c in _context.Grupos
                             select c;
                var cuentas = from c in _context.Cuenta
                              select c;

                // buscar egresos
                var egresos = _context.Gastos;
                // buscar asientos de egresos de las cuentas del condominio
                var asientos = from a in _context.LdiarioGlobals
                               join c in _context.CodigoCuentasGlobals
                               on a.IdCodCuenta equals c.IdCodCuenta
                               where c.IdCondominio == idCondominio && a.Fecha.Month == DateTime.Today.Month
                               select a;

                var asientosEgresos = from a in asientos
                                      join e in egresos
                                      on a.IdAsiento equals e.IdAsiento
                                      select a;
                // buscar ingresos
                var ingresos = _context.Ingresos;
                // buscar asientos de ingresos de las cuentas del condominio
                var asientosIngresos = from a in asientos
                                       join i in ingresos
                                       on a.IdAsiento equals i.IdAsiento
                                       select a;
                // cargar fecha
                var fecha = DateTime.Today;
                // calcular totales y diferencia
                var totalIngreso = asientosIngresos.Where(c => c.TipoOperacion == false).Sum(c => c.Monto);
                var totalEgreso = asientosEgresos.Where(c => c.TipoOperacion == true).Sum(c => c.Monto);
                var diferencia = totalIngreso - totalEgreso;
                //llenar modelo
                // inicializar modelo
                var modelo = new EstadoResultadoVM
                {
                    Egresos = await egresos.ToListAsync(),
                    Ingresos = await ingresos.ToListAsync(),
                    AsientosEgresos = await asientosEgresos.ToListAsync(),
                    AsientosIngresos = await asientosIngresos.ToListAsync(),
                    AsientosCondominio = await asientos.ToListAsync(),
                    SubCuentas = await subcuentas.ToListAsync(),
                    Cuentas = await cuentas.ToListAsync(),
                    Grupos = await grupos.ToListAsync(),
                    Clases = await clases.ToListAsync(),
                    Fecha = fecha,
                    TotalEgresos = totalEgreso,
                    TotalIngresos = totalIngreso,
                    Difenrencia = diferencia
                };

                return View(modelo);
            }
            catch (Exception ex)
            {
                var error = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", error);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        private async Task<RecibosCreadosVM> LoadDataDeudores(int idCondominio)
        {
            // CARGAR PROPIEDADES DE CADA INMUEBLE DEL CONDOMINIO
            var inmueblesCondominio = from c in _context.Inmuebles
                                      where c.IdCondominio == idCondominio
                                      select c;
            var propiedades = from c in _context.Propiedads
                              where c.Solvencia == false
                              select c;
            var propietarios = from c in _context.AspNetUsers
                               select c;
            var relacionesGastos = from c in _context.RelacionGastos
                                   where c.IdCondominio == idCondominio
                                   select c;
            var recibosCobro = from c in _context.ReciboCobros
                               select c;

            if (inmueblesCondominio != null && inmueblesCondominio.Any()
                && propiedades != null && propiedades.Any()
                && propietarios != null && propietarios.Any()
                && relacionesGastos != null && relacionesGastos.Any())
            {

                IList<Propiedad> listaPropiedadesCondominio = new List<Propiedad>();
                IList<ReciboCobro> recibosCobroCond = new List<ReciboCobro>();
                // BUSCAR PROPIEADES DE LOS INMUEBLES

                foreach (var item in inmueblesCondominio)
                {
                    var propiedadsCond = await propiedades.Where(c => c.IdInmueble == item.IdInmueble).ToListAsync();
                    var aux2 = listaPropiedadesCondominio.Concat(propiedadsCond).ToList();
                    listaPropiedadesCondominio = aux2;
                }

                // BUSCAR SUS RECIBOS DE COBRO
                // BUSCAR PROPIEDADES CON DEUDA
                foreach (var propiedad in listaPropiedadesCondominio)
                {
                    var recibo = await recibosCobro.Where(c => c.IdPropiedad == propiedad.IdPropiedad
                                                            && c.EnProceso != true
                                                            && c.Pagado != true)
                                                    .ToListAsync();

                    var aux = recibosCobroCond.Concat(recibo).ToList();
                    recibosCobroCond = aux;
                }

                var modelo = new RecibosCreadosVM
                {
                    Propiedades = listaPropiedadesCondominio,
                    Propietarios = await propietarios.ToListAsync(),
                    Recibos = recibosCobroCond,
                    Inmuebles = await inmueblesCondominio.ToListAsync()
                };

                return modelo;
            }

            return new RecibosCreadosVM();
        }
    }
}