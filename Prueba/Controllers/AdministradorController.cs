
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = "RequireAdmin")]

    public class AdministradorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailService _serviceEmail;
        private readonly IManageExcel _manageExcel;
        private readonly PruebaContext _context;

        public AdministradorController(IUnitOfWork unitOfWork,
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
        public async Task<IActionResult> Index()
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
        public IActionResult Dashboard(int? id)
        {
            // GUARDAR ID CONDOMINIO PARA SELECCIONAR SUS CUENTAS CONTABLES
            TempData["idCondominio"] = id.ToString();
            return View();
        }
        public IActionResult CuentasContables()
        {
            //HACER MODELO PARA CARGAR TODAS LAS CUENTAS A LA TABLA INDEX
            var modelo = new IndexCuentasContablesVM();

            // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == idCondominio
                                       select c;

            //CONSULTAS A BD SOBRE CLASE - GRUPO - CUENTA - SUB CUENTA

            IQueryable<Clase> clases = from c in _context.Clases
                                       select c;
            IQueryable<Grupo> grupos = from c in _context.Grupos
                                       select c;
            IQueryable<Cuenta> cuentas = from c in _context.Cuenta
                                         select c;
            IQueryable<SubCuenta> subcuentas = from c in _context.SubCuenta
                                               select c;

            IList<SubCuenta> subcuentasModel = new List<SubCuenta>();

            foreach (var item in cuentasContablesCond)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (item.IdCodigo == subcuenta.Id)
                    {
                        subcuentasModel.Add(subcuenta);
                    }
                }
            }

            //PASAR MODELO
            modelo.Clases = clases.ToList();
            modelo.Grupos = grupos.ToList();
            modelo.Cuentas = cuentas.ToList();
            modelo.SubCuentas = subcuentasModel;
            //CREAR FOR PARA CREAR LAS FILAS CON LA INFO 

            TempData.Keep();
            return View(modelo);
        }
        public IActionResult CrearSubCuenta()
        {
            SubcuentaCascadingVM modelo = new SubcuentaCascadingVM();

            var clases = from c in _context.Clases
                         select c;

            var clasesModel = clases.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() });

            modelo.Clases = clasesModel.ToList();

            return View(modelo);
        }
        [HttpPost]
        public async Task<JsonResult> AjaxMethod(string tipo, int valor)
        {
            SubcuentaCascadingVM model = new SubcuentaCascadingVM();
            switch (tipo)
            {
                case "IdClase":
                    var grupos = from c in _context.Grupos
                                 where c.IdClase == valor
                                 select c;

                    model.Grupos = await grupos.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() }).ToListAsync();
                    break;
                case "IdGrupo":
                    var cuentas = from c in _context.Cuenta
                                  where c.IdGrupo == valor
                                  select c;

                    model.Cuentas = await cuentas.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() }).ToListAsync();
                    break;
            }
            return Json(model);
        }

        public IActionResult CrearSubCuentaPost(SubcuentaCascadingVM modelo)
        {
            if (ModelState.IsValid)
            {
                // REGISTRAR SUB CUENTA CON IDCUENTA, DESCRIP Y CODIGO

                var nuevaSubCuenta = new SubCuenta
                {
                    IdCuenta = modelo.IdCuenta,
                    Descricion = modelo.Descripcion,
                    Codigo = modelo.Codigo
                };

                using (var _dbContext = new PruebaContext())
                {
                    _dbContext.Add(nuevaSubCuenta);
                    _dbContext.SaveChanges();
                }

                // REGISTRAR EN CUENTAS CONTABLES GLOBAL ID CONDOMINIO Y ID SUB CUENTA
                //recuperar el id del condominio
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var nuevoCC = new CodigoCuentasGlobal
                {
                    IdCodigo = nuevaSubCuenta.Id,
                    IdCondominio = idCondominio
                };

                using (var _dContext = new PruebaContext())
                {
                    _dContext.Add(nuevoCC);
                    _dContext.SaveChanges();
                }

                return RedirectToAction("CuentasContables");

            }

            return View(modelo);
        }

        public async Task<IActionResult> IndexPagosEmitidos()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = new IndexPagosVM();

            var listaPagos = from c in _context.PagoEmitidos
                             where c.IdCondominio == idCondominio
                             select c;

            var referencias = from r in _context.ReferenciasPes
                              select r;

            IList<ReferenciasPe> referenciaModel = new List<ReferenciasPe>();
            foreach (var pago in listaPagos)
            {
                if (pago.FormaPago)
                {
                    foreach (var referencia in referencias)
                    {
                        if (pago.IdPagoEmitido == referencia.IdPagoEmitido)
                        {
                            referenciaModel.Add(referencia);
                        }
                        continue;
                    }
                }
                continue;
            }
            modelo.PagosEmitidos = await listaPagos.ToListAsync();
            modelo.Referencias = referenciaModel;


            return View(modelo);
        }
        public IActionResult RegistrarPagos()
        {
            var modelo = new RegistroPagoVM();
            //LLENAR SELECT DE SUBCUENTAS DE GASTOS

            //traer subcuentas del condominio
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == idCondominio
                                       select c;

            //CONSULTAS A BD SOBRE CLASE - GRUPO - CUENTA - SUB CUENTA

            //CARGAR SELECT DE SUB CUENTAS DE GASTOS
            IQueryable<Grupo> gruposGastos = from c in _context.Grupos
                                             where c.IdClase == 5
                                             select c;

            IQueryable<Cuenta> cuentas = from c in _context.Cuenta
                                         select c;

            IQueryable<SubCuenta> subcuentas = from c in _context.SubCuenta
                                               select c;

            //CARGAR SELECT DE SUB CUENTAS DE BANCOS
            IQueryable<Cuenta> bancos = from c in _context.Cuenta
                                        where c.Descripcion.ToUpper().Trim() == "BANCO"
                                        select c;
            IQueryable<Cuenta> caja = from c in _context.Cuenta
                                      where c.Descripcion.ToUpper().Trim() == "CAJA"
                                      select c;

            IQueryable<SubCuenta> subcuentasBancos = from c in _context.SubCuenta
                                                     where c.IdCuenta == bancos.FirstOrDefault().Id
                                                     select c;
            IQueryable<SubCuenta> subcuentasCaja = from c in _context.SubCuenta
                                                   where c.IdCuenta == caja.FirstOrDefault().Id
                                                   select c;

            IList<Cuenta> cuentasGastos = new List<Cuenta>();
            foreach (var grupo in gruposGastos)
            {
                foreach (var cuenta in cuentas)
                {
                    if (cuenta.IdGrupo == grupo.Id)
                    {
                        cuentasGastos.Add(cuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasGastos = new List<SubCuenta>();
            foreach (var cuenta in cuentasGastos)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (subcuenta.IdCuenta == cuenta.Id)
                    {
                        subcuentasGastos.Add(subcuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasModel = new List<SubCuenta>();
            foreach (var condominioCC in cuentasContablesCond)
            {
                foreach (var subcuenta in subcuentasGastos)
                {
                    if (condominioCC.IdCodCuenta == subcuenta.Id)
                    {
                        subcuentasModel.Add(subcuenta);
                    }
                    continue;
                }
            }


            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            // ENVIAR MODELO

            TempData.Keep();

            return View(modelo);
        }

        [HttpPost]
        public IActionResult RegistrarPagosPost(RegistroPagoVM modelo)
        {
            modelo.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            if (ModelState.IsValid)
            {
                // TRAER EL ID DEL CONDOMINIO
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
                // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
                PagoEmitido pago = new PagoEmitido
                {
                    IdCondominio = idCondominio,
                    Fecha = modelo.Fecha,
                    Monto = modelo.Monto,
                };
                if (modelo.Pagoforma == FormaPago.Efectivo)
                {
                    pago.FormaPago = false;

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(pago);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                    //buscar el id en codigo de cuentas global de la subcuenta seleccionada
                    var diario = from l in _context.LdiarioGlobals
                                 select l;

                    int numAsiento = 1;

                    if (diario.Count() > 0)
                    {
                        numAsiento = diario.ToList().LastOrDefault().NumAsiento;
                    }

                    LdiarioGlobal asientoGasto = new LdiarioGlobal
                    {
                        IdCodCuenta = modelo.IdSubcuenta,
                        Fecha = modelo.Fecha,
                        Concepto = modelo.Concepto,
                        Monto = modelo.Monto,
                        TipoOperacion = false,
                        NumAsiento = numAsiento + 1
                    };
                    LdiarioGlobal asientoCaja = new LdiarioGlobal
                    {
                        IdCodCuenta = modelo.IdCodigoCuentaCaja,
                        Fecha = modelo.Fecha,
                        Concepto = modelo.Concepto,
                        Monto = modelo.Monto,
                        TipoOperacion = true,
                        NumAsiento = numAsiento + 1
                    };

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(asientoGasto);
                        _dbContext.Add(asientoCaja);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN LA TABLA GASTOS
                    Gasto gasto = new Gasto
                    {
                        IdAsiento = asientoGasto.IdAsiento
                    };
                    //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                    Activo activo = new Activo
                    {
                        IdAsiento = asientoCaja.IdAsiento
                    };

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(gasto);
                        _dbContext.Add(activo);
                        _dbContext.SaveChanges();
                    }
                }
                else if (modelo.Pagoforma == FormaPago.Transferencia)
                {
                    pago.FormaPago = true;

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(pago);
                        _dbContext.SaveChanges();
                    }

                    ReferenciasPe referencia = new ReferenciasPe
                    {
                        IdPagoEmitido = pago.IdPagoEmitido,
                        NumReferencia = modelo.NumReferencia,
                        Banco = modelo.IdCodigoCuentaBanco.ToString()
                    };

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(referencia);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                    //buscar el id en codigo de cuentas global de la subcuenta seleccionada
                    var diario = from l in _context.LdiarioGlobals
                                 select l;

                    int numAsiento = 1;

                    if (diario.Count() > 0)
                    {
                        numAsiento = diario.ToList().LastOrDefault().NumAsiento + 1;
                    }

                    LdiarioGlobal asientoGasto = new LdiarioGlobal
                    {
                        IdCodCuenta = modelo.IdSubcuenta,
                        Fecha = modelo.Fecha,
                        Concepto = modelo.Concepto,
                        Monto = modelo.Monto,
                        TipoOperacion = true,
                        NumAsiento = numAsiento
                    };
                    LdiarioGlobal asientoBanco = new LdiarioGlobal
                    {
                        IdCodCuenta = modelo.IdCodigoCuentaBanco,
                        Fecha = modelo.Fecha,
                        Concepto = modelo.Concepto,
                        Monto = modelo.Monto,
                        TipoOperacion = false,
                        NumAsiento = numAsiento
                    };

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(asientoGasto);
                        _dbContext.Add(asientoBanco);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN LA TABLA GASTOS
                    Gasto gasto = new Gasto
                    {
                        IdAsiento = asientoGasto.IdAsiento
                    };
                    //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                    Activo activo = new Activo
                    {
                        IdAsiento = asientoBanco.IdAsiento
                    };

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(gasto);
                        _dbContext.Add(activo);
                        _dbContext.SaveChanges();
                    }

                }

                return RedirectToAction("IndexPagosEmitidos");

            }
            return RedirectToAction("RegistrarPagos");

        }

        public async Task<IActionResult> RelaciondeGastos()
        {
            // CARGAR GASTOS REGISTRADOS

            // BUSCAR FONDOS Y PREVISIONES POR idCODCUEENTASGLOBAL -> ID CONDOMINIO

            //traer subcuentas del condominio
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await LoadDataRelacionGastos(idCondominio);

            // BUSCAR PROVISIONES en los asientos del diario
            var proviciones = from p in _context.Provisiones
                              select p;
            // BUSCAR FONDOS
            var fondos = from f in _context.Fondos
                         select f;

            var subcuentas = from c in _context.SubCuenta
                             select c;

            if (proviciones.Any() && fondos.Any())
            {
                modelo.Provisiones = await proviciones.ToListAsync();
                modelo.Fondos = await fondos.ToListAsync();

                IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();
                foreach (var fondo in modelo.Fondos)
                {
                    var subcuentaFondo = subcuentas.Where(c => fondo.IdCodCuenta == c.Id);

                    if (subcuentaFondo.Any())
                    {
                        SubCuenta aux = subcuentaFondo.FirstOrDefault();
                        if (aux != null)
                        {
                            modelo.Total += modelo.SubTotal * (fondo.Porcentaje / 100);
                            subcuentasFondosModel.Add(aux);
                        }
                    }
                }
                IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                foreach (var provision in modelo.Provisiones)
                {
                    var subcuentaProvision = subcuentas.Where(c => provision.IdCodCuenta == c.Id);

                    if (subcuentaProvision.Any())
                    {
                        SubCuenta aux = subcuentaProvision.FirstOrDefault();
                        if (aux != null)
                        {
                            modelo.SubTotal += provision.Monto;
                            subcuentasProvisionesModel.Add(aux);
                        }
                    }
                }
                modelo.SubCuentasFondos = subcuentasFondosModel;
                modelo.SubCuentasProvisiones = subcuentasProvisionesModel;
            }
            else if (proviciones.Any() && !fondos.Any())
            {
                modelo.Provisiones = await proviciones.ToListAsync();

                IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                foreach (var provision in modelo.Provisiones)
                {
                    var subcuentaProvision = subcuentas.Where(c => provision.IdCodCuenta == c.Id);

                    if (subcuentaProvision.Any())
                    {
                        SubCuenta aux = subcuentaProvision.FirstOrDefault();
                        if (aux != null)
                        {
                            modelo.SubTotal += provision.Monto;
                            subcuentasProvisionesModel.Add(aux);
                        }
                    }
                }

                modelo.SubCuentasProvisiones = subcuentasProvisionesModel;

            }
            else if (!proviciones.Any() && fondos.Any())
            {
                modelo.Fondos = await fondos.ToListAsync();
                IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();

                foreach (var fondo in modelo.Fondos)
                {
                    var subcuentaFondo = subcuentas.Where(c => fondo.IdCodCuenta == c.Id);

                    if (subcuentaFondo.Any())
                    {
                        SubCuenta aux = subcuentaFondo.FirstOrDefault();
                        if (aux != null)
                        {
                            modelo.Total += modelo.SubTotal*fondo.Porcentaje/100;
                            subcuentasFondosModel.Add(aux);
                        }
                    }
                }
                modelo.SubCuentasFondos = subcuentasFondosModel;
            }

            TempData.Keep();
            return View(modelo);

        }

        public IActionResult CrearFondo()
        {
            var modelo = new CrearFondoVM();

            // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == idCondominio
                                       select c;

            IQueryable<Grupo> gruposPatrimonio = from c in _context.Grupos
                                                 where c.IdClase == 3
                                                 select c;
            IQueryable<Cuenta> cuentas = from c in _context.Cuenta
                                         where gruposPatrimonio.FirstOrDefault().Id == c.IdGrupo
                                         select c;
            IQueryable<SubCuenta> subcuentas = from c in _context.SubCuenta
                                               where cuentas.FirstOrDefault().Id == c.IdCuenta
                                               select c;

            // LLENAR SELECT CON LOS FONDOS REGISTRADOS
            modelo.Fondos = subcuentas.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

            TempData.Keep();

            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> CrearFondo(CrearFondoVM modelo)
        {
            if (ModelState.IsValid)
            {
                var fondo = new Fondo
                {
                    IdCodCuenta = modelo.IdFondo,
                    Porcentaje = modelo.Porcentaje
                };

                using (var db_context = new PruebaContext())
                {
                    await db_context.AddAsync(fondo);
                    await db_context.SaveChangesAsync();
                }

                return RedirectToAction("RelaciondeGastos");
            }
            return View(modelo);
        }

        public IActionResult CrearProvision()
        {
            var modelo = new CrearProvisionVM();

            // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == idCondominio
                                       select c;
            var gruposGastos = from c in _context.Grupos
                               where c.IdClase == 5
                               select c;

            var cuentaProvision = from c in _context.Cuenta
                                 where c.Descripcion.Trim().ToUpper() == "PROVISIONES"
                                 select c;

            var subcuentasProvisiones = from c in _context.SubCuenta
                                        where c.IdCuenta == cuentaProvision.FirstOrDefault().Id
                                        select c;

            var cuentas = from c in _context.Cuenta
                          select c;

            var subcuentas = from c in _context.SubCuenta
                             select c;

            // CARGAR CUENTAS GASTOS DEL CONDOMINIO
            IList<Cuenta> cuentasGastos = new List<Cuenta>();
            foreach (var grupo in gruposGastos)
            {
                foreach (var cuenta in cuentas)
                {
                    if (cuenta.IdGrupo == grupo.Id)
                    {
                        cuentasGastos.Add(cuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasGastos = new List<SubCuenta>();
            foreach (var cuenta in cuentasGastos)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (subcuenta.IdCuenta == cuenta.Id)
                    {
                        subcuentasGastos.Add(subcuenta);
                    }
                    continue;
                }
            }

            modelo.Gastos = subcuentasGastos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.Provisiones = subcuentasProvisiones.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> CrearProvision(CrearProvisionVM modelo)
        {
            if (ModelState.IsValid)
            {
                // GUARDAR PREVISION EN BD
                var provision = new Provision
                {
                    IdCodGasto = modelo.IdGasto,
                    IdCodCuenta = modelo.IdcodCuenta,
                    Monto = modelo.Monto
                };

                // CREAR ASIENTO SOBRE PREVISION

                var diario = from l in _context.LdiarioGlobals
                             select l;

                int numAsiento = 1;

                if (diario.Count() > 0)
                {
                    numAsiento = diario.ToList().LastOrDefault().NumAsiento + 1;
                }

                LdiarioGlobal asientoProvision = new LdiarioGlobal
                {
                    IdCodCuenta = provision.IdCodCuenta,
                    Fecha = DateTime.Today,
                    Concepto = modelo.Concepto,
                    Monto = modelo.Monto,
                    TipoOperacion = false,
                    NumAsiento = numAsiento
                };
                LdiarioGlobal asientoGastoProvisionado = new LdiarioGlobal
                {
                    IdCodCuenta = modelo.IdGasto,
                    Fecha = DateTime.Today,
                    Concepto = modelo.Concepto,
                    Monto = modelo.Monto,
                    TipoOperacion = true,
                    NumAsiento = numAsiento
                };
                using (var db_context = new PruebaContext())
                {
                    await db_context.AddAsync(provision);
                    await db_context.AddAsync(asientoProvision);
                    await db_context.AddAsync(asientoGastoProvisionado);
                    await db_context.SaveChangesAsync();
                }

                return RedirectToAction("RelaciondeGastos");

            }


            return View(modelo);
        }

        // cargar relacion de gastos dependiendo del condominio
        private async Task<RelacionDeGastosVM> LoadDataRelacionGastos(int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);

            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == condominio.IdCondominio
                                       select c;

            var gruposGastos = from c in _context.Grupos
                               where c.IdClase == 5
                               select c;

            var cuentas = from c in _context.Cuenta
                          select c;

            var subcuentas = from c in _context.SubCuenta
                             select c;

            // CARGAR DIARIO COMPLETO
            var diario = from d in _context.LdiarioGlobals
                         select d;

            // CARGAR CUENTAS GASTOS DEL CONDOMINIO
            IList<Cuenta> cuentasGastos = new List<Cuenta>();
            foreach (var grupo in gruposGastos)
            {
                foreach (var cuenta in cuentas)
                {
                    if (cuenta.IdGrupo == grupo.Id)
                    {
                        cuentasGastos.Add(cuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasGastos = new List<SubCuenta>();
            foreach (var cuenta in cuentasGastos)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (subcuenta.IdCuenta == cuenta.Id)
                    {
                        subcuentasGastos.Add(subcuenta);
                    }
                    continue;
                }
            }

            // BUSCAR ASIENTOS EN EL DIARIO CORRESPONDIENTES A LAS CUENTAS GASTOS DEL CONDOMINIO
            IList<LdiarioGlobal> asientosGastosCondominio = new List<LdiarioGlobal>();
            IList<SubCuenta> subcuentasModel = new List<SubCuenta>();
            decimal subtotal = 0;
            decimal total = 0;
            foreach (var asiento in diario)
            {
                foreach (var gasto in subcuentasGastos)
                {
                    if (asiento.IdCodCuenta == gasto.Id)
                    {
                        subtotal += asiento.Monto;
                        total += asiento.Monto;
                        asientosGastosCondominio.Add(asiento);
                        subcuentasModel.Add(gasto);
                    }
                    continue;
                }
            }
            // CREAR MODELO PARA LOS TOTALES DE LA RELACION DE GASTOS Y CARGAR VISTA
            var modelo = new RelacionDeGastosVM
            {
                GastosDiario = asientosGastosCondominio,
                SubcuentasGastos = subcuentasModel,
                Total = total,
                SubTotal = subtotal,
                Fecha = DateTime.Today,
                Condominio = condominio
            };

            return modelo;
        }

        // generar recibos de cobro
        // generar pdf

        public IActionResult PagosRecibidos()
        {
            return View();
        }
        public IActionResult LibroDiario()
        {

            //traer subcuentas del condominio
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == idCondominio
                                       select c;

            var subcuentas = from c in _context.SubCuenta
                             select c;
            var clases = from c in _context.Clases
                         select c;
            var grupos = from c in _context.Grupos
                         select c;
            var cuentas = from c in _context.Cuenta
                          select c;
            // CARGAR DIARIO COMPLETO
            var diario = from d in _context.LdiarioGlobals
                         select d;

            // BUSCAR ASIENTOS CORRESPONDIENTES A LAS SUBCUENTAS DEL CONDOMINIO
            // CALCULAR EL TOTAL DEL DEBE Y HABER Y SU DIFERENCIA

            IList<LdiarioGlobal> asientosCondominio = new List<LdiarioGlobal>();
            IList<SubCuenta> subCuentasModel = new List<SubCuenta>();
            decimal totalDebe = 0;
            decimal totalHaber = 0;

            foreach (var asiento in diario)
            {
                foreach (var ccCondominio in cuentasContablesCond)
                {
                    if (asiento.IdCodCuenta == ccCondominio.IdCodCuenta)
                    {
                        asientosCondominio.Add(asiento);
                        var aux = subcuentas.Where(c => c.Id == asiento.IdCodCuenta).ToList();
                        subCuentasModel.Add(aux.FirstOrDefault());
                        if (asiento.TipoOperacion)
                        {
                            totalDebe += asiento.Monto;
                        }
                        else
                        {
                            totalHaber += asiento.Monto;
                        }
                    }
                    continue;
                }

            }

            decimal diferencia = totalDebe - totalHaber;

            // LLENAR MODELO

            var modelo = new LibroDiarioVM
            {
                AsientosCondominio = asientosCondominio,
                CuentasDiarioCondominio = subCuentasModel,
                Clases = clases.ToList(),
                Grupos = grupos.ToList(),
                Cuentas = cuentas.ToList(),
                TotalDebe = totalDebe,
                TotalHaber = totalHaber,
                Diferencia = diferencia
            };
            TempData.Keep();

            return View(modelo);
        }
        public IActionResult Deudores()
        {
            return View();
        }
    }
}
