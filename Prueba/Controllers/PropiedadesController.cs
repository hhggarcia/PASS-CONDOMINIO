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
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class PropiedadesController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailService _emailServices;
        private readonly IPdfReportesServices _servicesPDF;
        private readonly NuevaAppContext _context;

        public PropiedadesController(IPdfReportesServices servicesPDF,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            NuevaAppContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _emailServices = emailService;
            _servicesPDF = servicesPDF;
        }

        // GET: Propiedades
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Propiedads.Include(p => p.IdCondominioNavigation).Include(p => p.IdUsuarioNavigation).Where(p => p.IdCondominio == IdCondominio);

            TempData.Keep();

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
        public async Task<IActionResult> Create([Bind("IdPropiedad,IdCondominio,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda,MontoIntereses,MontoMulta,Creditos")] Propiedad propiedad)
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

                TempData["IDPropiedad"] = propiedad.IdPropiedad.ToString();

                TempData.Keep();

                return RedirectToAction(nameof(Grupos));
            }
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", propiedad.IdCondominio);
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", propiedad.IdUsuario);

            TempData.Keep();

            return View(propiedad);
        }


        /// <summary>
        /// ver los grupos a los que pertenece una propiedad
        /// </summary>
        /// <param name="id">Id de la propiedad</param>
        /// <returns></returns>
        public IActionResult VerGrupos(int id)
        {
            var gruposDePropiedad = from c in _context.GrupoGastos
                                    join d in _context.PropiedadesGrupos
                                    on c.IdGrupoGasto equals d.IdGrupoGasto
                                    where d.IdPropiedad == id
                                    select c;

            TempData["IdPropiedad"] = id.ToString();

            return View(gruposDePropiedad);
        }

        /// <summary>
        /// Eliminar un grupo de una propiedad
        /// </summary>
        /// <param name="id">Id del grupo a eliminar</param>
        /// <returns></returns>
        public async Task<IActionResult> EliminarDeGrupo(int id)
        {
            var idPropiedad = Convert.ToInt32(TempData.Peek("IdPropiedad").ToString());

            //var propiedadGrupo = from c in _context.PropiedadesGrupos
            //                     where c.IdPropiedad == idPropiedad && c.IdGrupoGasto == id
            //                     select c;

            var propiedadGrupo = await _context.PropiedadesGrupos
                .Include(p => p.IdGrupoGastoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == idPropiedad && m.IdGrupoGasto == id);

            if (propiedadGrupo == null)
            {
                return NotFound();
            }

            TempData.Keep();

            return View(propiedadGrupo);
        }



        /// <summary>
        /// Confirmacion
        /// </summary>
        /// <param name="id">Id de la relacion grupo gasto - propiedad</param>
        /// <returns></returns>
        [HttpPost, ActionName("EliminarDeGrupo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDeGrupoConfirmed(int id)
        {
            var idPropiedad = Convert.ToInt32(TempData.Peek("IdPropiedad").ToString());

            //var propiedadGrupo = await _context.PropiedadesGrupos.FindAsync(id);
            var propiedadGrupo = await _context.PropiedadesGrupos
                .Include(p => p.IdGrupoGastoNavigation)
                .Include(p => p.IdPropiedadNavigation)
                .FirstOrDefaultAsync(m => m.IdPropiedad == idPropiedad && m.IdGrupoGasto == id);

            if (propiedadGrupo == null)
            {
                return NotFound();
            }
            if (propiedadGrupo == null)
            {
                return NotFound();
            }

            _context.PropiedadesGrupos.Remove(propiedadGrupo);
            await _context.SaveChangesAsync();

            return RedirectToAction("VerGrupos", new { id = propiedadGrupo.IdPropiedad });
        }

        /// <summary>
        /// carga los grupos existentes
        /// </summary>
        /// <returns></returns>
        public IActionResult Grupos()
        {
            var grupos = _context.GrupoGastos.ToList();

            var modelo = grupos.Select(grupo => 
            new PropiedadGruposVM
            {
                Text = grupo.NombreGrupo,
                Value = grupo.IdGrupoGasto.ToString(),
                Selected = false,
                Alicuota = 0
            }
            ).ToList();

            return View(modelo);
        }

        /// <summary>
        /// registra los grupos de gastos a los que pertenece la propiedad
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarGrupos(List<PropiedadGruposVM> model)
        {
            int idPropiedad = Convert.ToInt32(TempData.Peek("IDPropiedad").ToString());

            var propiedad = await _context.Propiedads.FindAsync(idPropiedad);

            if (propiedad == null)
            {
                return NotFound();
            }

            foreach (var item in model)
            {
                if (item.Selected)
                {
                    // buscar grupo
                    var aux = Convert.ToInt32(item.Value);
                    var grupo = await _context.GrupoGastos.FindAsync(aux);

                    if (grupo == null)
                    {
                        return NotFound();
                    }
                    // guardar relacion propiedad-grupo
                    var propiedadGrupo = new PropiedadesGrupo()
                    {
                        IdGrupoGasto = grupo.IdGrupoGasto,
                        IdPropiedad = propiedad.IdPropiedad,
                        Alicuota = item.Alicuota
                    };

                    _context.PropiedadesGrupos.Add(propiedadGrupo);
                }
            }

            await _context.SaveChangesAsync();

            TempData.Keep();

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Descargar PDF del estado de cuenta 
        /// de la propiedad especifica
        /// </summary>
        /// <param name="id">Id de la propiedad</param>
        /// <returns>PDF</returns>
        public async Task<IActionResult> EstadoCuentaPropiedad(int id)
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var condominio = await _context.Condominios.FindAsync(idCondominio);

            if (condominio != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(id);
                var modelo = new List<EstadoCuentasVM>();

                if (propiedad != null)
                {
                    var usuario = await _context.AspNetUsers.FirstAsync(c => c.Id == propiedad.IdUsuario);
                    var recibos = await _context.ReciboCobros.
                        Where(c => c.IdPropiedad == propiedad.IdPropiedad && !c.Pagado)
                        .OrderBy(c => c.Fecha)
                        .ToListAsync();

                    modelo.Add(new EstadoCuentasVM()
                    {
                        Condominio = condominio,
                        Propiedad = propiedad,
                        User = usuario,
                        ReciboCobro = recibos
                    });

                    TempData.Keep();
                    var data = _servicesPDF.EstadoCuentas(modelo);
                    Stream stream = new MemoryStream(data);
                    return File(stream, "application/pdf", "EstadoCuentasOficina_" + propiedad.Codigo + "_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
                }

            }
            return View("Index");
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
        public async Task<IActionResult> Edit(int id, [Bind("IdPropiedad,IdCondominio,IdUsuario,Codigo,Dimensiones,Alicuota,Solvencia,Saldo,Deuda,MontoIntereses,MontoMulta,Creditos")] Propiedad propiedad)
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
                // buscar usuario
                var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);
                // propiedades grupos
                var propiedadesGrupos = await _context.PropiedadesGrupos.Where(p => p.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                // recibo cobro
                var recibosPropiedad = await _context.ReciboCobros.Where(p => p.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                // recibo cuotas
                var recibosCuotas = await _context.ReciboCuotas.Where(p => p.IdPropiedad == propiedad.IdPropiedad).ToListAsync();
                // pago recibido
                //var pagoRecibidos = await _context.PagoRecibidos.Where(p => p.IdPropiedad == propiedad.IdPropiedad).ToListAsync();

                //var recibos = await _context.ReciboCobros.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();

                var pagoRecibidos = await (from p in _context.PagoRecibidos
                                   join cc in _context.PagosRecibos
                                   on p.IdPagoRecibido equals cc.IdPago
                                   join r in _context.ReciboCobros
                                   on cc.IdRecibo equals r.IdReciboCobro
                                   select p).ToListAsync();

                _context.PropiedadesGrupos.RemoveRange(propiedadesGrupos);
                _context.ReciboCobros.RemoveRange(recibosPropiedad);
                _context.ReciboCuotas.RemoveRange(recibosCuotas);
                _context.PagoRecibidos.RemoveRange(pagoRecibidos);
                _context.AspNetUsers.Remove(usuario);
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
