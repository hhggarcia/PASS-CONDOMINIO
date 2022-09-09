using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Areas.Identity.Data;
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

        public AdminController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
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

            return RedirectToAction("RegistroInmueble", modelo);
        }

        [HttpGet]
        public IActionResult RegistroInmueble(NuevoCondominio modelo)
        {
            return View(modelo);
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
        public IActionResult RegistrarEstacionamiento()
        {
            return View();
        }
        public IActionResult RegistrarPuesto()
        {
            return View();
        }
        public IActionResult RegistrarPropiedades()
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
