using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Core.ViewModels;
using Prueba.Services;
using Prueba.Utils;
using Prueba.ViewModels;
using System.Collections;
using System.Web;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireSuperAdmin")]

    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailService _serviceEmail;
        private readonly IManageExcel _manageExcel;
        private readonly NuevaAppContext _context;

        public AdminController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail,
            IManageExcel manageExcel,
            NuevaAppContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _serviceEmail = serviceEmail;
            _manageExcel = manageExcel;
            _context = context;
        }

        /* ETIQUETA
         * Metodo para crear un usuario Administrador y
         * todos los propietarios de un condominio
         * enviar correo al finalizar los la creacion del condominio
         */

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            var users = _unitOfWork.User.GetUsers();
            ViewData["Beneficiario"] = users.First().NormalizedUserName;

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

        public async  Task<IActionResult> Condominio()
        {
            try
            {
                //CARGAR LIST DE CONDOMINIOS
                var condominios = _context.Condominios.Include(c => c.IdAdministradorNavigation);

                //foreach (var item in condominios)
                //{
                //    var inmuebles = _context.Inmuebles.Include(c => c.IdCondominioNavigation)
                //        .Where(c => c.IdInmueble == item.IdCondominio);
                //}

                var condominiosModel = await condominios.ToListAsync();
                return View(condominiosModel);

            }
            catch (Exception ex)
            {
                return View(new ErrorViewModel { RequestId = ex.Message });
            }
           
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
        /*POR HACER
         * VALIDAR EXCEL DE ERRORES EN TABLA
         * MOSTRAR ERRORES
         * VALIDAR SI LOS USUARIOS YA EXISTEN
         */
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuarios(NuevoCondominio modelo)
        {
            if (ModelState.IsValid)
            {
                //Extraer del excel los usuario
                var usuarios = _manageExcel.ExcelUsuarios(modelo.ExcelPropietarios);

                //CREAR ADMINISTRADOR
                var user = CreateUser();
                user.FirstName = modelo.Administrador.FirstName;
                user.LastName = modelo.Administrador.LastName;
                await _userStore.SetUserNameAsync(user, modelo.Administrador.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, modelo.Administrador.Email, CancellationToken.None);

                //CREAR
                var resultAdminCreate = await _userManager.CreateAsync(user, modelo.Administrador.Password);
                //VERIFICAR SI LA CONTRASE;A CUMPLE LOS REQUISITOS
                if (resultAdminCreate.Succeeded)
                {
                    //AGREGAR ROL DE ADMINISTRADOR 
                    //AddToRoleAsync para añadir un rol (usuario, "Rol")
                    await _signInManager.UserManager.AddToRoleAsync(user, "Administrador");

                    //GUARDAR EN COOKIE EL ADMINISTRADOR DEL NUEVO CONDOMINIO
                    TempData["Administrador"] = modelo.Administrador.Email;
                }
                else
                {
                    foreach (var error in resultAdminCreate.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(modelo);
                }

                //FOR LOOP PARA REGISTRAR A LOS PROPIETARIOS
                TempData["numPropietarios"] = usuarios.Count().ToString();
                for (int i = 0; i < usuarios.Count(); i++)
                {
                    var userPro = CreateUser();
                    userPro.FirstName = usuarios[i].FirstName;
                    userPro.LastName = usuarios[i].LastName;
                    await _userStore.SetUserNameAsync(userPro, usuarios[i].Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(userPro, usuarios[i].Email, CancellationToken.None);

                    //CREAR
                    var resultPropietarioCreate = await _userManager.CreateAsync(userPro, usuarios[i].Password);
                    //VERIFICAR SI LA CONTRASE;A CUMPLE LOS REQUISITOS
                    if (resultPropietarioCreate.Succeeded)
                    {
                        //AGREGAR ROL DE Propietario
                        //AddToRoleAsync para añadir un rol (usuario, "Rol")
                        await _signInManager.UserManager.AddToRoleAsync(userPro, "Propietario");

                        // ENVIAR CORREO DE NOTIFICACIÓN DE CREACIÓN DE LA CUENTA
                        //var correo = new RegisterConfirm
                        //{
                        //    To = Input.Email,
                        //    Subject = "Registro Condominio Password Technology",
                        //    Body = "Bienvenido a nuestra aplicación para administrar sus condominios."
                        //};

                        //_serviceEmail.SendEmail(correo);

                        //agregar a Temp propietarios
                        //guardar en TempData la lista de propietarios
                        string nombreTempData = "Propietarios" + i.ToString();
                        TempData[nombreTempData] = userPro.Email;
                    }
                    else
                    {
                        foreach (var error in resultPropietarioCreate.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return View(modelo);
                    }
                }

                TempData.Keep();

                return View("CrearCondominio", modelo);
            }

            return View(modelo);

        }


        public IActionResult PerfilUsuario()
        {
            return View();
        }
        public IActionResult CrearUsuarios()
        {
            return View();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
