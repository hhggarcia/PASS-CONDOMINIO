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
        private readonly PruebaContext _context;

        public PropietariosController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            PruebaContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
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

        public IActionResult DashboardUsuario()
        {
            return View();
        }

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
                                       where c.IdCuenta == bancos.FirstOrDefault().Id
                                       select c;

                var subcuentasCaja = from c in _context.SubCuenta
                                     where c.IdCuenta == caja.FirstOrDefault().Id
                                     select c;

                var propiedad = await _context.Propiedads.FindAsync(valor);

                modelo.Saldo = propiedad.Saldo;

                modelo.Deuda = propiedad.Deuda;


                modelo.SubCuentasBancos = await subcuentasBancos.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() }).ToListAsync();
                modelo.SubCuentasCaja = await subcuentasCaja.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() }).ToListAsync();
                modelo.Recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == valor && c.EnProceso == false && c.Pagado == false)
                                                            .OrderBy(c => c.Fecha).ToListAsync();
            }

            return Json(modelo);
        }
        public IActionResult Comprobante()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarPagos(PagoRecibidoVM modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var propiedades = await _context.Propiedads.Where(c => c.IdPropiedad == modelo.IdPropiedad).ToListAsync();
                    var inmuebles = await _context.Inmuebles.Where(c => c.IdInmueble == propiedades.First().IdInmueble).ToListAsync();
                    var condominio = await _context.Condominios.Where(c => c.IdCondominio == inmuebles.First().IdCondominio).ToListAsync();
                    var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == modelo.IdPropiedad && c.EnProceso == false).OrderBy(c => c.Fecha).ToListAsync();

                    var comprobante = new ComprobanteVM()
                    {
                        Propiedad = propiedades.First(),
                        Inmueble = inmuebles.First(),
                        Condominio = condominio.First(),
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
                            Confirmado = false
                        };

                        pago.FormaPago = true;

                        var banco = await _context.SubCuenta.Where(c => c.Id == modelo.IdCodigoCuentaBanco).ToListAsync();

                        switch (modelo.DeudaPagar)
                        {
                            case 1:
                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.First().Saldo;

                                    if (monto > 0 && recibos.Count() > 0)
                                    {
                                        pago.Monto = monto;
                                    }
                                    else
                                    {
                                        comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                        return View("Comprobante", comprobante);
                                    }
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
                                    Banco = banco.First().Descricion
                                };

                                // regitrar en bd la referencia del pago
                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia);
                                    dbcontext.SaveChanges();
                                }

                                // cambiar a -en proceso = true- el recibo mas actual 
                                var reciboActual = recibos.First();
                                reciboActual.EnProceso = true;

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReciboCobros.Update(reciboActual);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia;
                                comprobante.Mensaje = "Gracias por su pago!";

                                return View("Comprobante", comprobante);
                            case 2:

                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.First().Deuda;
                                    if (monto > 0 && recibos.Count() > 0)
                                    {
                                        pago.Monto = monto;
                                    }
                                    else
                                    {
                                        comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                        return View("Comprobante", comprobante);
                                    }
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                var referencia2 = new ReferenciasPr()
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    NumReferencia = modelo.NumReferencia,
                                    Banco = banco.First().Descricion
                                };

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia2);
                                    dbcontext.SaveChanges();
                                }

                                // cambiar a -en proceso = true- de los recibos 
                                
                                using (var dbcontext = new PruebaContext())
                                {
                                    foreach (var item in recibos.Skip(0))
                                    {
                                        item.EnProceso = true;
                                        dbcontext.ReciboCobros.Update(item);
                                    }
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia2;
                                comprobante.Mensaje = "Gracias por su pago!";

                                return View("Comprobante", comprobante);

                            case 3:

                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.First().Saldo + propiedades.First().Deuda;
                                    if (monto > 0 && recibos.Count() > 0)
                                    {
                                        pago.Monto = monto;
                                    }
                                    else
                                    {
                                        comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                        return View("Comprobante", comprobante);
                                    }
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                var referencia3 = new ReferenciasPr()
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    NumReferencia = modelo.NumReferencia,
                                    Banco = banco.First().Descricion
                                };

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia3);
                                    dbcontext.SaveChanges();
                                }
                                // cambiar a -en proceso = true- de los recibos 

                                using (var dbcontext = new PruebaContext())
                                {
                                    foreach (var item in recibos)
                                    {
                                        item.EnProceso = true;
                                        dbcontext.ReciboCobros.Update(item);
                                    }
                                    dbcontext.SaveChanges();
                                }
                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia3;
                                comprobante.Mensaje = "Gracias por su pago!";

                                return View("Comprobante", comprobante);

                            default:
                                return RedirectToAction("RegistrarPagos");
                        }
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

                        switch (modelo.DeudaPagar)
                        {
                            case 1:
                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.First().Saldo;

                                    if (monto > 0 && recibos.Count() > 0)
                                    {
                                        pago.Monto = monto;
                                    }
                                    else
                                    {
                                        comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                        return View("Comprobante", comprobante);
                                    }
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }
                                // cambiar a -en proceso = true- el recibo mas actual 
                                var reciboActual = recibos.First();
                                reciboActual.EnProceso = true;

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReciboCobros.Update(reciboActual);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Mensaje = "Gracias por su pago!";

                                return View("Comprobante", comprobante);

                            case 2:

                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.First().Deuda;

                                    if (monto > 0 && recibos.Count() > 0)
                                    {
                                        pago.Monto = monto;
                                    }
                                    else
                                    {
                                        comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                        return View("Comprobante", comprobante);
                                    }
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                // cambiar a -en proceso = true- de los recibos 

                                using (var dbcontext = new PruebaContext())
                                {
                                    foreach (var item in recibos.Skip(0))
                                    {
                                        item.EnProceso = true;
                                        dbcontext.ReciboCobros.Update(item);
                                    }
                                    dbcontext.SaveChanges();
                                }
                                comprobante.PagoRecibido = pago;
                                comprobante.Mensaje = "Gracias por su pago!";

                                return View("Comprobante", comprobante);

                            case 3:
                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.First().Saldo + propiedades.First().Deuda;

                                    if (monto > 0 && recibos.Count() > 0)
                                    {
                                        pago.Monto = monto;
                                    }
                                    else
                                    {
                                        comprobante.Mensaje = "No existe deuda en la propiedad seleccionada";
                                        return View("Comprobante", comprobante);
                                    }
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                // cambiar a -en proceso = true- de los recibos 

                                using (var dbcontext = new PruebaContext())
                                {
                                    foreach (var item in recibos)
                                    {
                                        item.EnProceso = true;
                                        dbcontext.ReciboCobros.Update(item);
                                    }
                                    dbcontext.SaveChanges();
                                }
                                comprobante.PagoRecibido = pago;
                                comprobante.Mensaje = "Gracias por su pago!";
                                return View("Comprobante", comprobante);

                            default:
                                return RedirectToAction("RegistrarPagos");
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

        public async Task<IList<int>> IdsCondominios(string IdUsuario)
        {
            var propiedades = from c in _context.Propiedads
                              where c.IdUsuario == IdUsuario
                              select c;

            List<int> listIdCondominios = new List<int>();

            if (propiedades != null && propiedades.Count() > 0)
            {
                foreach (var item in propiedades)
                {
                    var inmueble = await _context.Inmuebles.FindAsync(item.IdInmueble);

                    if (inmueble != null)
                    {
                        listIdCondominios.Add(inmueble.IdCondominio);
                    }
                }

                return listIdCondominios;
            }

            return listIdCondominios;
        }
    }
}
