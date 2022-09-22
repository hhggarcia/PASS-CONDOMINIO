
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public JsonResult GetListGrupo(int value)
        {
            var grupos = _context.Grupos.Where(x => x.IdClase == value).ToList();

            return Json(new SelectList(grupos,"Id", "Descripcion"));
        }
        
        public JsonResult GetListCuentas(int value)
        {
            var cuentas = _context.Cuenta.Where(x => x.IdGrupo == value).ToList();

            return Json(new SelectList(cuentas, "Id", "Descripcion"));
        }


        public IActionResult CrearSubCuentaPost(int claseId, int grupoId, int cuentaId)
        {
            var modelo = new SubCuentaCrearVM();

            var clases = from c in _context.Clases
                         where c.Id == claseId
                         select c;
            modelo.Clase = clases.FirstOrDefault();

            var grupos = from c in _context.Grupos
                         where c.Id == grupoId
                         select c;
            modelo.Grupo = grupos.FirstOrDefault();

            var cuentas = from c in _context.Cuenta
                          where c.Id == cuentaId
                          select c;
            modelo.Cuenta = cuentas.FirstOrDefault();

            return View(modelo);
        }

        public IActionResult RegistrarPagos()
        {
            return View();
        }
        public IActionResult PagosRecibidos()
        {
            return View();
        }
        public IActionResult LibroDiario()
        {
            return View();
        }
        public IActionResult Deudores()
        {
            return View();
        }
        public IActionResult RelaciondeGastos()
        {
            return View();
        }

    }
}
