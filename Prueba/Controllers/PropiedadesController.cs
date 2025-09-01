using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Models;
using Prueba.Services;
using Prueba.ViewModels;
using Prueba.Repositories;

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
        private readonly ICondominioRepository _repoCondominio;
        private readonly NuevaAppContext _context;

        public PropiedadesController(ICondominioRepository repoCondominio,
            IPdfReportesServices servicesPDF,
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
            _repoCondominio = repoCondominio;
        }

        // GET: Propiedades
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Propiedads
                .Include(p => p.IdCondominioNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .Where(p => p.IdCondominio == IdCondominio);

            TempData.Keep();

            return View(await nuevaAppContext.ToListAsync());
        }

        public async Task<IActionResult> IndexUserPropiedades()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var propiedades = await _repoCondominio.GetPropiedadesCondominio(IdCondominio);
            var model = propiedades.GroupBy(c => c.IdUsuarioNavigation).ToList();

            TempData.Keep();
            return View(model);
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

        public async Task<IActionResult> Inquilino(int id)
        {
            var propiedad = await _context.Propiedads.FindAsync(id);
            if (propiedad == null)
            {
                return NotFound();
            }

            var nuevaAppContext = _context.Inquilinos
                .Include(i => i.IdPropiedadNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .Where(c => c.IdPropiedad == propiedad.IdPropiedad);
            TempData["idUserInquilinos"] = propiedad.IdUsuario;
            TempData.Keep();

            return View(await nuevaAppContext.ToListAsync());
        }

        public IActionResult CreateInquilino()
        {
            string idPropietario = TempData.Peek("idUserInquilinos").ToString();

            ViewData["IdPropiedad"] = new SelectList(_context.Propiedads.Where(c => c.IdUsuario == idPropietario), "IdPropiedad", "Codigo");

            TempData.Keep();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInquilino([Bind("Nombre,Rif,Email,Password,ConfirmPassword,Telefono,IdPropiedad")] RegistrarInquilinoVM modelo)
        {
            var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
            if (propiedad != null)
            {
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

                    return RedirectToAction("Index");

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
        // GET: Propiedades/Create
        public IActionResult Create(AspNetUser usuario)
        {
            //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers.Where(c => c.Id == usuario.Id).ToList(), "Id", "Email");
            return View();
        }

        public IActionResult CreatePropiedad()
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var model = new CrearUserPropiedadesVM();
            var grupos = _context.GrupoGastos.Where(c => c.IdCondominio == idCondominio).ToList();

            model.AvailableExpenseGroups = grupos.Select(c => new GrupoVM()
            {
                Id = c.IdGrupoGasto,
                Nombre = c.NombreGrupo,
                Alicuota = 0
            }).ToList();

            TempData.Keep();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePropiedad(CrearUserPropiedadesVM model)
        {
            if (ModelState.IsValid)
            {
                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var condominio = await _context.Condominios.FindAsync(IdCondominio);
                if (condominio != null)
                {
                    // VALIDAR 100% SUMA DE ALICUOTAS DE LAS PROPIEDADES

                    string returnUrl = Url.Content("~/");

                    // PASO 2 - REGISTRAR DATOS DE ADMINISTRADOR

                    var user = CreateUser();

                    user.FirstName = model.Nombre;
                    user.LastName = model.Rif;

                    await _userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        //AGREGAR ROL DE PROPIETARIO 
                        //AddToRoleAsync para añadir un rol (usuario, "Rol")
                        await _signInManager.UserManager.AddToRoleAsync(user, "Propietario");

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        var msg = $"Por favor, confirmar su cuenta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>.";

                        // PASO 3 - REGISTRAR PROPIEDADES
                        foreach (var propiedadVM in model.Propiedades)
                        {
                            var propiedadNueva = new Propiedad()
                            {
                                IdCondominio = condominio.IdCondominio,
                                IdUsuario = user.Id,
                                Codigo = propiedadVM.Codigo,
                                Dimensiones = 0,
                                Alicuota = propiedadVM.Alicuota,
                                Saldo = propiedadVM.Saldo,
                                Deuda = propiedadVM.Deuda,
                                MontoIntereses = 0,
                                MontoMulta = 0,
                                Creditos = 0,
                                Solvencia = !(propiedadVM.Saldo > 0 || propiedadVM.Deuda > 0)
                            };

                            _context.Propiedads.Add(propiedadNueva);
                            await _context.SaveChangesAsync();

                            // registrar la relacion PropiedadesGrupos de Gastos

                            foreach (var grupoGastoSelect in propiedadVM.Grupos)
                            {
                                var propiedadGrupo = new PropiedadesGrupo()
                                {
                                    IdGrupoGasto = grupoGastoSelect.Id,
                                    IdPropiedad = propiedadNueva.IdPropiedad,
                                    Alicuota = grupoGastoSelect.Alicuota
                                };

                                _context.PropiedadesGrupos.Add(propiedadGrupo);
                            }

                        }

                        await _context.SaveChangesAsync();

                        // ENVIAR CORREO DE CONFIRMACION DE CUENTA
                        //var resultCorreo = _emailServices.ConfirmEmail("g.hector9983@gmail.com", user.Email, "rrmbjahggwhvkrgi", msg);
                        var resultCorreo = _emailServices.ConfirmEmail(condominio.Email,
                            user.Email ?? "",
                            condominio.ClaveCorreo ?? "",
                            msg);

                        if (!resultCorreo.Contains("OK"))
                        {
                            var modeloError = new ErrorViewModel()
                            {
                                RequestId = resultCorreo
                            };

                            TempData.Keep();
                            return RedirectToPage("Error", modeloError);
                        }

                        TempData.Keep();
                        return RedirectToAction("IndexUserPropiedades");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData.Keep();
                    return View(model);
                }
            }
            TempData.Keep();
            return View(model);
        }

        // POST: Propiedades/Create
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


        public async Task<IActionResult> HistoricoPagosPropiedad()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            //var model = new List<HistoricoPropiedadPagosVM>();
            // buscar propiedades
            var propiedades = await _repoCondominio.GetPropiedadesCondominio(IdCondominio);
            TempData.Keep();
            return View(propiedades);
        }

        public async Task<IActionResult> DetallePagosPropiedad(int id)
        {
            var propiedad = await _context.Propiedads.FindAsync(id);
            var model = new HistoricoPropiedadPagosVM();

            if (propiedad != null)
            {
                // buscar pagos Propiedad
                // buscar pagos
                // buscar referencia si aplica

                var pagosPropiedad = await _context.PagoPropiedads
                .Where(c => c.IdPropiedad == propiedad.IdPropiedad)
                .Include(c => c.IdPagoNavigation)
                    .ThenInclude(c => c.ReferenciasPrs)
                .ToListAsync();

                // cargar modelo
                model.Propiedad = propiedad;
                model.Pagos = pagosPropiedad;
            }

            return View(model);
        }

        public async Task<IActionResult> PdfHistoricoPagosPropiedad(int id)
        {
            var propiedad = await _context.Propiedads.FindAsync(id);
            var model = new HistoricoPropiedadPagosVM();

            if (propiedad != null)
            {
                // buscar pagos Propiedad
                // buscar pagos
                // buscar referencia si aplica

                var pagosPropiedad = await _context.PagoPropiedads
                .Where(c => c.IdPropiedad == propiedad.IdPropiedad)
                .Include(c => c.IdPagoNavigation)
                    .ThenInclude(c => c.ReferenciasPrs)
                .ToListAsync();

                // cargar modelo
                model.Propiedad = propiedad;
                model.Pagos = pagosPropiedad;

                var data = _servicesPDF.HistoricoPagosPropiedadPDF(model);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "HistoricoPagos_" + propiedad.Codigo + "_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }

            return View("HistoricoPagosPropiedad");
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
            ViewData["IdUsuario"] = new SelectList(_context.AspNetUsers, "Id", "Email", propiedad.IdUsuario);
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
