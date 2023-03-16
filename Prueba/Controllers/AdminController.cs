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
        private readonly PruebaContext _context;

        public AdminController(IUnitOfWork unitOfWork,
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

                foreach (var item in condominios)
                {
                    var inmuebles = _context.Inmuebles.Include(c => c.IdCondominioNavigation)
                        .Where(c => c.IdInmueble == item.IdCondominio);
                }

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

        [HttpGet]
        public IActionResult CrearCondominio(NuevoCondominio modelo)
        {
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> CrearCondominioPost(NuevoCondominio modelo)
        {
            try
            {
                //extraer adminstrador email de tempdata
                string emailAdmin = TempData.Peek("Administrador").ToString();
                //buscar administrador por email
                ApplicationUser admin = await _signInManager.UserManager.FindByEmailAsync(emailAdmin);
                var condominio = new Condominio
                {
                    Nombre = modelo.Condominio.Nombre,
                    IdAdministrador = admin.Id,
                    Rif = modelo.Condominio.Rif,
                    Tipo = modelo.Condominio.Tipo

                };

                await _context.Condominios.AddAsync(condominio);

                await _context.SaveChangesAsync();

                //LLENAR INFORMACION DE LOS SELECTS EN INMUEBLES

                TempData["IdCondominio"] = condominio.IdCondominio.ToString();

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

                TempData.Keep();

                return View("RegistroInmueble", modelo);
            }
            catch (Exception ex)
            {
                return View(new ErrorViewModel { RequestId = ex.Message });
            }            

        }

        [HttpGet]
        public IActionResult RegistroInmueble(NuevoCondominio modelo)
        {
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> RegistroInmueblePost(NuevoCondominio modelo)
        {
            try
            {
                //Recuperar ID del condominio

                var condominio = from c in _context.Condominios
                                 where c.IdCondominio == Convert.ToInt32(TempData.Peek("IdCondominio").ToString())
                                 select c;

                //LLENAR SELECT DE USUARIOS PARA ASIGNAR PROPIEDADES
                IList<ApplicationUser> usuarios = new List<ApplicationUser>();

                for (int i = 0; i < Convert.ToInt32(TempData["numPropietarios"]); i++)
                {
                    string nombreTempData = "Propietarios" + i.ToString();
                    string emailProp = TempData.Peek(nombreTempData).ToString();

                    ApplicationUser prop = await _signInManager.UserManager.FindByEmailAsync(emailProp);

                    if (prop != null)
                    {
                        usuarios.Add(prop);
                    }
                }

                var propietariosModel = usuarios.Select(u => new SelectListItem(u.Email, u.Id));

                modelo.Propietarios = propietariosModel;
                //// REGISTRAR EL INMUEBLE

                Inmueble inmueble = new Inmueble
                {
                    IdCondominio = condominio.FirstOrDefault().IdCondominio,
                    IdZona = modelo.Ubicacion.IdZona,
                    Nombre = modelo.Inmueble.Nombre,
                    TotalPropiedad = modelo.Inmueble.TotalPropiedad
                };

                using (var dbcontext = new PruebaContext())
                {
                    await dbcontext.Inmuebles.AddAsync(inmueble);
                    await dbcontext.SaveChangesAsync();

                }

                //GUARDAR EN TEMPDATA EL ID DEL INMUEBLE CREADO
                TempData["IdInmueble"] = inmueble.IdInmueble.ToString();
                ViewBag.numPropInmueble = inmueble.TotalPropiedad.ToString();


                TempData.Keep();


                return View("RegistrarPropiedades", modelo);

            }
            catch (Exception ex)
            {
                return View(new ErrorViewModel { RequestId = ex.Message });
            }           
        }

        [HttpGet]
        public IActionResult RegistrarPropiedades(NuevoCondominio modelo)
        {
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarPropiedadesPost(NuevoCondominio modelo)
        {
            try
            {
                //RECUPERAR ID DEL INMUEBLE
                var idInmueble = Convert.ToInt32(TempData.Peek("IdInmueble").ToString());

                //REGISTRAR PROPIEDADES EN BD
                foreach (var propiedad in modelo.Propiedades)
                {
                    propiedad.IdInmueble = idInmueble;

                    await _context.Propiedads.AddAsync(propiedad);

                }

                await _context.SaveChangesAsync();

                //  GUARDAR PROPIEDADES PARA SABER SI SE CREARA UN ESTACIONAMIENTO Y ASIGNAR LOS PUESTOS
                TempData["numPropiedades"] = modelo.Propiedades.Count().ToString();

                for (int i = 0; i < modelo.Propiedades.Count(); i++)
                {
                    string nombreTempData = "Propiedad" + i.ToString();

                    TempData[nombreTempData] = modelo.Propiedades[i].IdPropiedad.ToString();
                }

                TempData.Keep();

                return View("RegistrarEstacionamiento", modelo);
            }
            catch (Exception ex)
            {
                return View(new ErrorViewModel { RequestId = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult RegistrarEstacionamiento(NuevoCondominio modelo)
        {
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarEstacionamientoPost(NuevoCondominio modelo)
        {
            try
            {
                var idInmueble = Convert.ToInt32(TempData.Peek("IdInmueble").ToString());

                modelo.Estacionamiento.IdInmueble = idInmueble;

                //REGISTRAR ESTACIONAMIENTO CON EL ID DEL INMUEBLE

                using (var dbcontext = new PruebaContext())
                {
                    await dbcontext.Estacionamientos.AddAsync(modelo.Estacionamiento);
                    await dbcontext.SaveChangesAsync();

                }
                //GUARDAR ID DE ESTACIONAMIENTO EN TEMP DATA
                TempData["idEstacionamiento"] = modelo.Estacionamiento.IdEstacionamiento.ToString();
                TempData["numPuestos"] = modelo.Estacionamiento.NumPuestos.ToString();

                //LLENAR SELECT DE PROPIEDADES
                IList<Propiedad> propiedadesDb = new List<Propiedad>();

                for (int i = 0; i < Convert.ToInt32(TempData.Peek("numPropiedades")); i++)
                {
                    string nombreTempData = "Propiedad" + i.ToString();

                    var idPropiedad = Convert.ToInt32(TempData.Peek(nombreTempData));

                    var propiedad = await _context.Propiedads.FindAsync(idPropiedad);

                    if (propiedad != null)
                    {
                        propiedadesDb.Add(propiedad);
                    }
                }

                var propiedadesModel = propiedadesDb.Select(p => new SelectListItem(p.Codigo, p.IdPropiedad.ToString()));

                modelo.SelectPropiedades = propiedadesModel;

                return View("RegistrarPuestosE", modelo);
            }
            catch (Exception ex)
            {
                return View(new ErrorViewModel { RequestId = ex.Message });
            }
            
        }

        [HttpGet]
        public IActionResult RegistrarPuestosE(NuevoCondominio modelo)
        {
            return View(modelo);
        }

        public async Task<IActionResult> RegistrarPuestosEPost(NuevoCondominio modelo)
        {
            try
            {
                //RECUPERAR ID DEL ESTACIONAMIENTO
                var idEstacionamiento = Convert.ToInt32(TempData.Peek("IdEstacionamiento").ToString());

                //REGISTRAR PUESTOS EN BD
                foreach (var puesto in modelo.Puesto_Est)
                {
                    puesto.IdEstacionamiento = idEstacionamiento;

                    await _context.PuestoEs.AddAsync(puesto);

                }

                await _context.SaveChangesAsync();

                return View("Condominio");

            }
            catch (Exception ex)
            {
                return View(new ErrorViewModel { RequestId = ex.Message });
            }
            
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
