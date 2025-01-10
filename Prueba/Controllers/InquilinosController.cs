using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly NuevaAppContext _context;
        private readonly IEmailService _emailServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;


        public InquilinosController(IEmailService emailService,
                                    UserManager<ApplicationUser> userManager,
                                    IUserStore<ApplicationUser> userStore,
                                    SignInManager<ApplicationUser> signInManager,
                                    ICuentasContablesRepository repoCuentas,
                                    NuevaAppContext context)
        {
            _emailServices = emailService;
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _repoCuentas = repoCuentas;
            _context = context;
        }

        // GET: Inquilinos
        public async Task<IActionResult> Index()
        {
            string idPropietario = TempData.Peek("idUserLog").ToString();

            var nuevaAppContext = _context.Inquilinos
                .Include(i => i.IdPropiedadNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .Where(c => c.IdPropiedadNavigation.IdUsuario == idPropietario);

            TempData.Keep();

            return View(await nuevaAppContext.ToListAsync());
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                string idPropietario = TempData.Peek("idUserLog").ToString();

                var usuario = await _context.AspNetUsers.FindAsync(idPropietario);

                var inquilino = await _context.Inquilinos.FirstOrDefaultAsync(i => i.IdUsuario == idPropietario);

                if (inquilino != null)
                {
                    var propiedades = await _context.Propiedads
                    .Include(c => c.IdCondominioNavigation)
                    .Include(c => c.ReciboCobros)
                    .Where(c => c.IdPropiedad == inquilino.IdPropiedad)
                    .ToListAsync();

                    if (propiedades != null && propiedades.Any() && usuario != null)
                    {
                        TempData["idCondominio"] = propiedades.First().IdCondominioNavigation.IdCondominio;
                        var subcuentasBancos = await _repoCuentas.ObtenerBancos(propiedades.First().IdCondominioNavigation.IdCondominio);
                        var subcuentasCaja = await _repoCuentas.ObtenerCaja(propiedades.First().IdCondominioNavigation.IdCondominio);

                        TempData.Keep();
                        return View(new DashboardUsuarioVM()
                        {
                            Propiedades = propiedades,
                            Usuario = usuario,
                            SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList(),
                            SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList()
                        });
                    }
                }
                

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "Este usuario no tiene propiedades"
                };
                TempData.Keep();

                return View("Error", modeloError);
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

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos
                .Include(i => i.IdPropiedadNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdInquilino == id);
            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo");
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Inquilinos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdInquilino,IdUsuario,IdPropiedad,Rif,Telefono,Cedula,Activo")] Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inquilino);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", inquilino.IdPropiedad);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", inquilino.IdUsuario);
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos.FindAsync(id);
            if (inquilino == null)
            {
                return NotFound();
            }
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", inquilino.IdPropiedad);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", inquilino.IdUsuario);
            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInquilino,IdUsuario,IdPropiedad,Rif,Telefono,Cedula,Activo")] Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inquilino);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InquilinoExists(inquilino.IdInquilino))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads, "IdPropiedad", "Codigo", inquilino.IdPropiedad);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", inquilino.IdUsuario);
            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos
                .Include(i => i.IdPropiedadNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdInquilino == id);
            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inquilino = await _context.Inquilinos.FindAsync(id);
            if (inquilino != null)
            {
                var usuario = await _context.AspNetUsers.FindAsync(inquilino.IdUsuario);

                if (usuario != null) {
                    
                    _context.AspNetUsers.Remove(usuario);
                }
                _context.Inquilinos.Remove(inquilino);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateInquilino()
        {
            string idPropietario = TempData.Peek("idUserLog").ToString();

            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads.Where(c => c.IdUsuario == idPropietario), "IdPropiedad", "Codigo");

            TempData.Keep();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInquilino([Bind("Nombre,Rif,Email,Password,ConfirmPassword,Telefono,IdPropiedad")] RegistrarInquilinoVM modelo)
        {
            var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
            if (propiedad != null) {
                string returnUrl = Url.Content("~/");
                // crear usuario 
                var user = CreateUser();
                user.FirstName = modelo.Nombre;
                user.LastName = modelo.Rif;
                await _userStore.SetUserNameAsync(user, modelo.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, modelo.Email, CancellationToken.None);

                var password = modelo.Password ?? "Pass1234_";
                //CREAR
                var resultAdminCreate = await _userManager.CreateAsync(user, password);

                //VERIFICAR SI LA CONTRASE;A CUMPLE LOS REQUISITOS
                if (resultAdminCreate.Succeeded)
                {
                    //AGREGAR ROL DE ADMINISTRADOR 
                    //AddToRoleAsync para añadir un rol (usuario, "Rol")
                    await _signInManager.UserManager.AddToRoleAsync(user, "Inquilino");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var msg = $"Por favor, confirmar su cuenta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>.";

                    var inquilino = new Inquilino()
                    {
                        IdUsuario = userId,
                        IdPropiedad = propiedad.IdPropiedad,
                        Rif = modelo.Rif,
                        Telefono = modelo.Telefono,
                        Cedula = modelo.Rif,
                        Activo = true
                    };

                    // CREAR INQUILINO
                    _context.Inquilinos.Add(inquilino);
                    await _context.SaveChangesAsync();

                    var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);

                    // ENVIAR CORREO DE CONFIRMACION DE CUENTA
                    var resultCorreo = _emailServices.ConfirmEmail("g.hector9983@gmail.com", user.Email, "rrmbjahggwhvkrgi", msg);
                    //var resultCorreo = _emailServices.ConfirmEmail(condominio.Email, user.Email, condominio.ClaveCorreo, msg);

                    if (!resultCorreo.Contains("OK"))
                    {
                        var modeloError = new ErrorViewModel()
                        {
                            RequestId = resultCorreo
                        };

                        TempData.Keep();
                        return RedirectToPage("Error", modeloError);
                    }

                    return RedirectToAction("Inquilinos", "Propietarios");

                }
                else
                {
                    foreach (var error in resultAdminCreate.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(modelo);
                }
            }
            
            return View(modelo);

        }

        private bool InquilinoExists(int id)
        {
            return _context.Inquilinos.Any(e => e.IdInquilino == id);
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
