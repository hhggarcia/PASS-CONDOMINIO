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
using Prueba.ViewModels;
using System.Net;

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
        private readonly IPDFServices _servicePDF;
        private readonly IManageExcel _manageExcel;
        private readonly IReportesRepository _repoReportes;
        private readonly IRelacionGastoRepository _repoRelacionGasto;
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
            IPDFServices PDFService,
            IManageExcel manageExcel,
            IReportesRepository repoReportes,
            IRelacionGastoRepository repoRelacionGasto,
            PruebaContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _servicePDF = PDFService;
            _manageExcel = manageExcel;
            _repoReportes = repoReportes;
            _repoRelacionGasto = repoRelacionGasto;
            _context = context;
        }

        public IActionResult PruebaPDF()
        {
            var data = _servicePDF.ExamplePDF();
            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "detalleventa.pdf");
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
                var condominios = _context.Condominios.Include(c => c.IdAdministradorNavigation)
                    .Include(c => c.Inmuebles)
                    .Where(c => c.IdAdministrador == idAdministrador);

                foreach (var item in condominios)
                {
                    var inmuebles = _context.Inmuebles.Include(c => c.IdCondominioNavigation)
                        .Where(c => c.IdCondominio == item.IdCondominio);
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

                var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

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
                    //ReferenciasDolar = await _context.ReferenciaDolars.ToListAsync()
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

                        if (reciboActual == null || !reciboActual.Any())
                        {
                            _context.PagoRecibidos.Remove(pago);
                            await _context.SaveChangesAsync();

                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = "El Recibo asignado a este Pago fue eliminado! Este pago será eliminado"
                            };

                            return View("Error", modeloError);
                        }
                        // buscar referencia si tiene y eliminar pago

                        if (pago.FormaPago)
                        {
                            var referencia = await _context.ReferenciasPrs.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                            var pagosRecibo = await _context.PagosRecibos.Where(c => c.IdPago == pago.IdPagoRecibido).ToListAsync();

                            _context.ReferenciasPrs.RemoveRange(referencia);
                            _context.RemoveRange(pagosRecibo);
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

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                // buscar pago
                var pago = await _context.PagoRecibidos.FindAsync(id);

                if (pago != null)
                {
                    // buscar propiedad
                    var propiedad = await _context.Propiedads.FindAsync(pago.IdPropiedad);

                    if (propiedad != null)
                    {
                        var relacion = await _context.PagosRecibos.Where(c => c.IdPago == pago.IdPagoRecibido).ToListAsync();
                       
                        var reciboActual = (from r in relacion
                                            join rc in _context.ReciboCobros
                                            on r.IdRecibo equals rc.IdReciboCobro
                                            where r.IdPago == pago.IdPagoRecibido
                                            select rc).FirstOrDefault();

                        // buscar subcuenta contable donde esta el pago del condominio
                        var cuentaCondominio = from s in _context.SubCuenta
                                               join cc in _context.CodigoCuentasGlobals
                                               on s.Id equals cc.IdCodigo
                                               where cc.IdCondominio == idCondominio
                                               where s.IdCuenta == 15 && s.Codigo == "01"
                                               select s;

                        // buscar referencia si tiene
                        var referencias = new List<ReferenciasPr>();

                        if (pago.FormaPago)
                        {
                            referencias = await _context.ReferenciasPrs.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                        }

                        if (reciboActual == null)
                        {
                            _context.PagoRecibidos.Remove(pago);
                            await _context.SaveChangesAsync();

                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = "El Recibo asignado a este Pago fue eliminado! Este pago será eliminado"
                            };

                            return View("Error", modeloError);
                        }

                        try
                        {
                            // se pago solo el recibo del mes actual
                            using (var dbContext = new PruebaContext())
                            {

                                // ADD PAGOS ABONADOS SOBRE LOS RECIBOS
    
                                if (pago.MontoRef == reciboActual.Monto || propiedad.Saldo == pago.MontoRef || (pago.MontoRef ==(propiedad.Saldo + propiedad.Deuda))) //|| reciboActual.Abonado > reciboActual.Monto
                                {
                                    // SI EL MONTO PAGADO ES IGUAL AL DEL RECIBO 
                                    if(propiedad.Deuda == 0)
                                    {
                                        reciboActual.Abonado = reciboActual.Abonado - propiedad.Saldo;
                                        propiedad.Saldo = 0;
                                        propiedad.Solvencia = true;
                                        reciboActual.EnProceso = false;
                                        reciboActual.Pagado = true;
                                    }
                                    else
                                    {
                                        if(pago.MontoRef == propiedad.Deuda)
                                        {
                                            propiedad.Deuda = 0;
                                            reciboActual.Abonado = reciboActual.Abonado - propiedad.Deuda;
                                            if(reciboActual.Abonado == propiedad.Saldo)
                                            {
                                                reciboActual.Abonado =0;
                                                propiedad.Saldo = 0;
                                                propiedad.Solvencia = true;
                                                propiedad.Solvencia = true;
                                                reciboActual.EnProceso = false;
                                                reciboActual.Pagado = true;
                                            }
                                            else if(reciboActual.Abonado < propiedad.Saldo)
                                            {
                                                propiedad.Saldo = propiedad.Saldo - reciboActual.Abonado;
                                                reciboActual.Abonado = 0;
                                                propiedad.Solvencia = false;
                                                reciboActual.EnProceso = false;
                                                reciboActual.Pagado = false;
                                            }
                                        }
                                    }
                
                                }
                                //else if (pago.MontoRef < reciboActual.Monto)
                                else if ((pago.MontoRef < propiedad.Deuda)|| (propiedad.Deuda ==0 && propiedad.Saldo >pago.MontoRef))
                                {
                                    // SI EL MONTO PAGADO ES MENOR AL DEL RECIBO 

                                    // VERIFICAR SI HAY OTROS PAGOS ABONADOS

                                    // SI YA SE PAGA EL RECIBO, SI AUN QUEDA EN DEUDA, SI EL PAGO EXCEDE EL RECIBO
                                    if (propiedad.Deuda > pago.MontoRef)
                                    {
                                        propiedad.Deuda = propiedad.Deuda - pago.MontoRef;
                                    }
                                    else if(propiedad.Saldo > pago.MontoRef)
                                    {
                                        if(reciboActual.Abonado> propiedad.Saldo)
                                        {
                                            reciboActual.Abonado = reciboActual.Abonado - propiedad.Saldo;
                                        }
                                        propiedad.Saldo = propiedad.Saldo - pago.MontoRef;
                                        reciboActual.Abonado = 0;
                                    }
                                    propiedad.Solvencia = false;
                                    //reciboActual.EnProceso = true;
                                    reciboActual.Pagado = false;
                                }
                                else if (pago.MontoRef > reciboActual.Monto)
                                {
                                    // SI EL MONTO PAGADO ES MAYOR AL DEL RECIBO

                                    // PAGAR EL RECIBO

                                    // SI NO HAY DEUDA RESTAR EXCEDENTE AL SALDO

                                    // SI HAY DEUDA BUSCAR EL SIGUIENTE RECIBO Y VER SI ES PAGABLE CON EL MONTO
                   
                                    if(pago.MontoRef == reciboActual.Abonado)
                                    {
                                        reciboActual.Abonado =  pago.MontoRef - propiedad.Saldo;
                                    }
                                    else
                                    {
                                        reciboActual.Abonado = reciboActual.Abonado + (pago.MontoRef - propiedad.Saldo);
                                    }
                                    propiedad.Saldo = 0;
                                    propiedad.Solvencia = true;
                                    //reciboActual.EnProceso = false;
                                    reciboActual.Pagado = true;
                                }

                                //if (propiedad.Saldo == reciboActual.Monto
                                //    && propiedad.Deuda == 0)
                                //{
                                //    // hacer saldo = 0
                                //    propiedad.Saldo = 0;
                                //    // cambiar solvencia = True
                                //    propiedad.Solvencia = true;

                                //}
                                //else 
                                //if (propiedad.Saldo == 0
                                //    && propiedad.Deuda >= reciboActual.Monto)
                                //{
                                //    // restar de Deuda -Monto
                                //    propiedad.Deuda -= reciboActual.Monto;
                                //    // si deuda y saldo == 0 -> solvencia = true
                                //    if (propiedad.Deuda == 0)
                                //    {
                                //        propiedad.Solvencia = true;
                                //    }
                                //}
                                //else if (propiedad.Saldo > 0
                                //    && propiedad.Saldo != reciboActual.Monto
                                //    && propiedad.Deuda >= reciboActual.Monto)
                                //{
                                //    // restar de Deuda -Monto
                                //    propiedad.Deuda -= reciboActual.Monto;

                                //}

                                // cambiar en recibo o en los recibos - en proceso a false y pagado a true
                            

                                // cambiar pago.Confirmado a True
                                pago.Confirmado = true;

                                dbContext.Update(propiedad);
                                dbContext.Update(reciboActual);
                                dbContext.Update(pago);

                                int numAsiento = 1;

                                var diarioCondominio = from a in _context.LdiarioGlobals
                                                       join c in _context.CodigoCuentasGlobals
                                                       on a.IdCodCuenta equals c.IdCodCuenta
                                                       where c.IdCondominio == idCondominio
                                                       select a;

                                if (diarioCondominio.Count() > 0)
                                {
                                    numAsiento = diarioCondominio.ToList().Last().NumAsiento + 1;
                                }

                                // libro diario cuenta afecada
                                // asiento con los ingresos (Condominio) aumentar por haber Concepto (pago condominio -propiedad- -mes-)
                                // -> contra subcuenta  banco o caja por el debe
                                LdiarioGlobal asientoBanco = new LdiarioGlobal
                                {
                                    IdCodCuenta = pago.IdSubCuenta,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = true,
                                    NumAsiento = numAsiento + 1,
                                    MontoRef = pago.MontoRef,
                                    ValorDolar = pago.ValorDolar,
                                    SimboloMoneda = pago.SimboloMoneda,
                                    SimboloRef = pago.SimboloRef
                                    //IdDolar = reciboActual.First().IdDolar
                                };

                                LdiarioGlobal asientoIngreso = new LdiarioGlobal
                                {
                                    IdCodCuenta = cuentaCondominio.First().Id,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = false,
                                    NumAsiento = numAsiento + 1,
                                    MontoRef = pago.MontoRef,
                                    ValorDolar = pago.ValorDolar,
                                    SimboloMoneda = pago.SimboloMoneda,
                                    SimboloRef = pago.SimboloRef
                                    //IdDolar = reciboActual.First().IdDolar
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
                EstadoResultadoVM modelo = new EstadoResultadoVM
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
            //return View(relacionGasto);
            return View(modelo);
        }
       
      
        [HttpPost]
        public ContentResult ComprobantePagosRecibidosPDF([FromBody] IndexPagoRecibdioVM indexPagoRecibdio)
        {
            try
            {
                var data = _servicePDF.ComprobantePagosRecibidosPDF(indexPagoRecibdio);
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
        public async Task<IActionResult> PagosRecibidosPDF()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            IList<ApplicationUser> listaPropietarios = await _signInManager.UserManager.Users.ToListAsync();
            
            var propiedadesPorUsuario = new Dictionary<ApplicationUser, List<Propiedad>>();
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
            };
            var data = _servicePDF.ComprobantePagosRecibidosPDF(modelo);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "PagosRecibidos.pdf");
         }
        [HttpPost]
        public ContentResult EstadoResultadoPDF([FromBody] EstadoResultadoVM estadoResultado)
        {
            try
            {
                var data = _servicePDF.EstadoDeResultadoPDF(estadoResultado);
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

        public async Task<IActionResult> CuotasEspeciales()
        {
            try
            {
                var idAdministrador = TempData.Peek("idUserLog").ToString();
                var idCondominio = _context.Condominios.Where(c=> c.IdAdministrador == idAdministrador).Select(c=>c.IdCondominio).FirstOrDefault();
                var cuotasCondominio = _context.CuotasEspeciales.Where(c=>c.IdCondominio == idCondominio);
                var condominiosModel = await cuotasCondominio.ToListAsync();

                TempData.Keep();

                return View(cuotasCondominio);
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