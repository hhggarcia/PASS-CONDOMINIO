using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class PropiedadesController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailSender _emailSender;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly NuevaAppContext _context;

        public PropiedadesController(UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            NuevaAppContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailSender = emailSender;
            _emailStore = GetEmailStore();
        }

        // GET: Propiedades
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.Propiedads.Include(p => p.IdCondominioNavigation).Include(p => p.IdUsuarioNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Propiedades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == id);
            if (propiedad == null)
            {
                return NotFound();
            }

            return View(propiedad);
        }

        public IActionResult Inquilino()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInquilino([Bind("FirstName", "LastName", "Email", "Password")] Usuario usuario)
        {
            // crear usuario 
            var user = CreateUser();
            user.FirstName = usuario.FirstName;
            user.LastName = usuario.FirstName;
            await _userStore.SetUserNameAsync(user, usuario.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, usuario.Email, CancellationToken.None);

            var password = usuario.Password ?? "Pass1234_";
            //CREAR
            var resultAdminCreate = await _userManager.CreateAsync(user, password);
            //VERIFICAR SI LA CONTRASE;A CUMPLE LOS REQUISITOS
            if (resultAdminCreate.Succeeded)
            {
                //AGREGAR ROL DE ADMINISTRADOR 
                //AddToRoleAsync para añadir un rol (usuario, "Rol")
                await _signInManager.UserManager.AddToRoleAsync(user, "Propietario");
            }
            else
            {
                foreach (var error in resultAdminCreate.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(usuario);
            }

            var aspUser = _context.AspNetUsers.Where(c => c.Email == usuario.Email).ToList();

            if (aspUser.Count == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Create", aspUser[0]);
        }

        // GET: Propiedades/Create
        public IActionResult Create(AspNetUser usuario)
        {
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers.Where(c => c.Id == usuario.Id).ToList(), "Id", "Email");
            return View();
        }

        // POST: Propiedades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPropiedad,IdCondominio,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda,MontoIntereses")] Propiedad propiedad)
        {
            ModelState.Remove(nameof(propiedad.IdCondominioNavigation));
            ModelState.Remove(nameof(propiedad.IdUsuarioNavigation));

            if (ModelState.IsValid)
            {
                // traer id del condominio
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                // asignar el id del usuario y del condominio a la propiedad
                propiedad.IdCondominio = idCondominio;
                //propiedad.IdUsuario = user.Id;

                _context.Add(propiedad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Grupos));
            }
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", propiedad.IdCondominio);
            //ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", propiedad.IdUsuario);

            TempData.Keep();

            return View(propiedad);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Grupos()
        {
            var modelo = _context.GrupoGastos.Select(grupo => 
            new SelectListItem(
                grupo.NombreGrupo,
                grupo.IdGrupoGasto.ToString()));

            return View(modelo);
        }

        /// <summary>
        /// registra los grupos de gastos a los que pertenece la propiedad
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grupos(SelectListItem modelo)
        {
            var grupos = await _context.GrupoGastos.ToListAsync();

            return RedirectToAction("Index");
        }

        // GET: Propiedades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads.FindAsync(id);
            if (propiedad == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", propiedad.IdCondominio);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Id", propiedad.IdUsuario);
            return View(propiedad);
        }

        // POST: Propiedades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPropiedad,IdCondominio,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda,MontoIntereses")] Propiedad propiedad)
        {
            if (id != propiedad.IdPropiedad)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(propiedad.IdCondominioNavigation));
            ModelState.Remove(nameof(propiedad.IdUsuarioNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(propiedad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropiedadExists(propiedad.IdPropiedad))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", propiedad.IdCondominio);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", propiedad.IdUsuario);
            return View(propiedad);
        }

        // GET: Propiedades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedad = await _context.Propiedads
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == id);
            if (propiedad == null)
            {
                return NotFound();
            }

            return View(propiedad);
        }

        // POST: Propiedades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propiedad = await _context.Propiedads.FindAsync(id);
            if (propiedad != null)
            {
                _context.Propiedads.Remove(propiedad);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropiedadExists(int id)
        {
            return _context.Propiedads.Any(e => e.IdPropiedad == id);
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
