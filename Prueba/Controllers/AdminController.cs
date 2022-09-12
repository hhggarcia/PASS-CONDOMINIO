using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Core.ViewModels;
using Prueba.Models;
using Prueba.Services;
using Prueba.Utils;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailService _serviceEmail;
        private readonly IManageExcel _manageExcel;
        private readonly PruebaContext _context;

        public AdminController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            PruebaContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _context = dbContext;
        }

        /* ETIQUETA
         * Metodo para crear un usuario Administrador y
         * todos los propietarios de un condominio
         * enviar correo al finalizar los la creacion del condominio
         */

        public IActionResult Index()
        {
            var users = _unitOfWork.User.GetUsers();
            //var roles = _unitOfWork.Role.GetRoles();
            return View(users);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var user = _unitOfWork.User.GetUser(id);
            var roles = _unitOfWork.Role.GetRoles();

            var userRoles = await _signInManager.UserManager.GetRolesAsync(user);

            var roleItems = roles.Select(role =>
                new SelectListItem(
                    role.Name,
                    role.Id,
                    userRoles.Any(ur => ur.Contains(role.Name)))).ToList();

            var vm = new EditUserViewModel
            {
                User = user,
                Roles = roleItems
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(EditUserViewModel data)
        {
            var user = _unitOfWork.User.GetUser(data.User.Id);
            if (user == null)
            {
                return NotFound();
            }

            var userRolesInDb = await _signInManager.UserManager.GetRolesAsync(user);

            //Loop through the roles in ViewModel
            //Check if the Role is Assigned In DB
            //If Assigned -> Do Nothing
            //If Not Assigned -> Add Role

            var rolesToAdd = new List<string>();
            var rolesToDelete = new List<string>();

            foreach (var role in data.Roles)
            {
                var assignedInDb = userRolesInDb.FirstOrDefault(ur => ur == role.Text);
                if (role.Selected)
                {
                    if (assignedInDb == null)
                    {
                        rolesToAdd.Add(role.Text);
                    }
                }
                else
                {
                    if (assignedInDb != null)
                    {
                        rolesToDelete.Add(role.Text);
                    }
                }
            }

            if (rolesToAdd.Any())
            {
                await _signInManager.UserManager.AddToRolesAsync(user, rolesToAdd);
            }

            if (rolesToDelete.Any())
            {
                await _signInManager.UserManager.RemoveFromRolesAsync(user, rolesToDelete);
            }

            user.FirstName = data.User.FirstName;
            user.LastName = data.User.LastName;
            user.Email = data.User.Email;

            _unitOfWork.User.UpdateUser(user);

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpGet]
        public IActionResult RegistrarUsuarios()
        {
            return View();
        }
        /* POST LLENA MODELO
         * PARA CREAR CONDOMINIO
         * INFO DE ADMIN Y LISTA DE PROPIETARIOS
         */
        [HttpPost]
        public IActionResult RegistrarUsuarios(NuevoCondominio modelo)
        {
            //Extraer del excel los usuario
            var usuarios = _manageExcel.ExcelUsuarios(modelo.ExcelPropietarios);
            //verificar si el admin esta existe o no 

            //LLENAR INFORMACION DE LOS SELECTS EN INMUEBLES
            IQueryable<Zona> zonas = from z in _context.Zonas
                                     select z;

            IQueryable<Parroquia> parroquias = from p in _context.Parroquias
                                               select p;

            IQueryable<Municipio> municipios = from m in _context.Municipios
                                               select m;


            IQueryable<Estado> estados = from e in _context.Estados
                                         select e;

            IQueryable<Pais> pais = from p in _context.Pais
                                    select p;

            var paisModel = pais.Select(z => new SelectListItem(z.Nombre, z.IdPais.ToString()));
            var estadoModel = estados.Select(z => new SelectListItem(z.Nombre, z.IdEstado.ToString()));
            var municipioModel = municipios.Select(z => new SelectListItem(z.Municipio1, z.IdMunicipio.ToString()));
            var parroquiaModel = parroquias.Select(z => new SelectListItem(z.Parroquia1, z.IdParroquia.ToString()));
            var zonaModel = zonas.Select(z => new SelectListItem(z.Zona1, z.IdZona.ToString()));


            var ubicaciones = new Ubicacion
            {
                Paises = paisModel,
                Estados = estadoModel,
                Municipios = municipioModel,
                Parroquias = parroquiaModel,
                Zonas = zonaModel
            };

            modelo.Ubicacion = ubicaciones;

            return View("RegistroInmueble", modelo);
        }

        [HttpGet]
        public IActionResult RegistroInmueble(NuevoCondominio modelo)
        {
            return View(modelo);
        }

        public IActionResult RegistrarEstacionamiento(NuevoCondominio modelo)
        {
            return View(modelo);
        }
        public IActionResult RegistrarPuesto()
        {
            return View();
        }
        [HttpGet]
        public IActionResult RegistrarPropiedades()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RegistrarPropiedades2()
        {
            return View();
        }


        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult PerfilUsuario()
        {
            return View();
        }
        public IActionResult CrearUsuarios()
        {
            return View();
        }
        public IActionResult RegistrarPagos()
        {
            return View();
        }
        public IActionResult PagosRecibidos()
        {
            return View();
        }
        public IActionResult Edificio()
        {
            return View();
        }
        public IActionResult CrearCondominio()
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
