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
        public IActionResult Historial()
        {
            return View();
        }

        public IActionResult DashboardUsuario()
        {
            return View();
        }

        /*POR HACER...
         * Lista de propiedades
         * por cada propiedad -> Recibos pendientes y recibo actual (Mostrar en modal)
         * deuda vencida y deuda actual (En Propiedad)
         * 
         */
        public async Task<IActionResult> RegistrarPagos()
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

                var listaRecibos = _context.ReciboCobros.Where(c => c.IdPropiedad == valor && c.Pagado == false);

                modelo.SubCuentasBancos = await subcuentasBancos.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() }).ToListAsync();
                modelo.SubCuentasCaja = await subcuentasCaja.Select(c => new SelectListItem { Text = c.Descricion, Value = c.Id.ToString() }).ToListAsync();
                modelo.Recibos = await listaRecibos.ToListAsync();
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
                    var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == modelo.IdPropiedad).ToListAsync();

                    var comprobante = new ComprobanteVM()
                    {
                        Propiedad = propiedades.First(),
                        Inmueble = inmuebles.First(),
                        Condominio = condominio.First(),
                        FechaComprobante = DateTime.Today
                    };

                    var pago = new PagoRecibido()
                    {
                        IdPropiedad = modelo.IdPropiedad,
                        Fecha = modelo.Fecha,
                        IdSubCuenta = modelo.IdSubcuenta,
                        Concepto = modelo.Concepto
                    };

                    if (modelo.Pagoforma == FormaPago.Transferencia)
                    {
                        pago.FormaPago = true;
                        var banco = await _context.SubCuenta.Where(c => c.Id == modelo.IdSubcuenta).ToListAsync();

                        switch (modelo.DeudaPagar)
                        {
                            case 1:
                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.FirstOrDefault().Saldo;

                                    pago.Monto = monto;
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                var referencia = new ReferenciasPr()
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    NumReferencia = modelo.NumReferencia,
                                    Banco = banco.FirstOrDefault().Descricion
                                };

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia;

                                return RedirectToAction("Comprobante");
                            case 2:

                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.FirstOrDefault().Deuda;

                                    pago.Monto = monto;
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
                                    Banco = banco.FirstOrDefault().Descricion
                                };

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia2);
                                    dbcontext.SaveChanges();
                                }
                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia2;

                                return RedirectToAction("Comprobante");

                            case 3:

                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.FirstOrDefault().Saldo + propiedades.FirstOrDefault().Deuda;

                                    pago.Monto = monto;
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
                                    Banco = banco.FirstOrDefault().Descricion
                                };

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.ReferenciasPrs.Add(referencia3);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                comprobante.Referencias = referencia3;

                                return RedirectToAction("Comprobante");

                            default:
                                return RedirectToAction("RegistrarPagos");
                        }
                    }
                    else
                    {
                        pago.FormaPago = false;                        

                        switch (modelo.DeudaPagar)
                        {
                            case 1:
                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.FirstOrDefault().Saldo;

                                    pago.Monto = monto;
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;
                                return RedirectToAction("Comprobante");
                            case 2:

                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.FirstOrDefault().Deuda;

                                    pago.Monto = monto;
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;

                                return RedirectToAction("Comprobante");

                            case 3:
                                if (propiedades.Count() > 0)
                                {
                                    var monto = propiedades.FirstOrDefault().Saldo + propiedades.FirstOrDefault().Deuda;

                                    pago.Monto = monto;
                                }

                                using (var dbcontext = new PruebaContext())
                                {
                                    dbcontext.PagoRecibidos.Add(pago);
                                    dbcontext.SaveChanges();
                                }

                                comprobante.PagoRecibido = pago;

                                return RedirectToAction("Comprobante");

                            default:
                                return RedirectToAction("RegistrarPagos");
                        }
                    }
                }
                catch (Exception ex)
                {                    
                    return RedirectToAction("RegistrarPagos");
                }
            }

            return RedirectToAction("RegistrarPagos");
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
