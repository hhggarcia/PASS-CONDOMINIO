#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Services;
using Prueba.ViewModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Prueba.Models;

namespace Prueba.Areas.Identity.Pages.Account
{
    public class NuevoCondominioModel : PageModel
    {
        private readonly NuevaAppContext _context;
        private readonly IEmailService _servicesEmail;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public NuevoCondominioModel(NuevaAppContext context,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IEmailService servicesEmail)
        {
            _context = context;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _servicesEmail = servicesEmail;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The name field should have a maximum of 50 characters")]
            public string Nombre { get; set; } = string.Empty;
            [Required]
            [StringLength(50, ErrorMessage = "The Rif/CI field should have a maximum of 50 characters")]
            public string Rif { get; set; } = string.Empty;
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
            [Required]
            [StringLength(50, ErrorMessage = "The telefono field should have a maximum of 50 characters")]
            [DisplayName("Teléfono")]
            public string Telefono { get; set; } = string.Empty;
            [Required]
            [DisplayName("Rif Condominio")]
            [StringLength(50, ErrorMessage = "The Rif field should have a maximum of 50 characters")]
            public string RifCondominio { get; set; } = string.Empty;

            public string Tipo { get; set; } = null!;
            [Required]
            [StringLength(50, ErrorMessage = "The Rif field should have a maximum of 50 characters")]
            [DisplayName("Razón Social")]
            public string NombreCondominio { get; set; } = string.Empty;
            [Required]
            [DisplayName("% Mora")]
            public decimal InteresMora { get; set; }

            [Required]
            [DisplayName("Dirección")]
            public string Direccion { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string EmailCondominio { get; set; } = null!;

            public bool? ContribuyenteEspecial { get; set; }

            [Required]
            public decimal? Multa { get; set; }

            public string ClaveCorreo { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string returnUrl = Url.Content("~/");

                // PASO 2 - REGISTRAR DATOS DE ADMINISTRADOR

                var user = CreateUser();

                user.FirstName = Input.Nombre;
                user.LastName = Input.Rif;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    //AGREGAR ROL DE ADMINISTRADOR 
                    //AddToRoleAsync para añadir un rol (usuario, "Rol")
                    await _signInManager.UserManager.AddToRoleAsync(user, "Administrador");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var msg = $"Por favor, confirmar su cuenta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>.";

                    // PASO 3 - REGISTRAR DATOS DE CONDOMINIO

                    var condominio = new Condominio()
                    {
                        IdAdministrador = userId,
                        Rif = Input.RifCondominio,
                        Tipo = Input.Tipo,
                        Nombre = Input.Nombre,
                        InteresMora = Input.InteresMora,
                        Direccion = Input.Direccion,
                        Email = Input.EmailCondominio,
                        ContribuyenteEspecial = Input.ContribuyenteEspecial,
                        Multa = Input.Multa,
                        ClaveCorreo = Input.ClaveCorreo
                    };

                    _context.Condominios.Add(condominio);

                    var idNuevoCondominio = await _context.SaveChangesAsync();
                    _context.Administradors.Add(new Administrador()
                    {
                        IdCondominio = idNuevoCondominio,
                        IdUsuario = userId,
                        Cargo = "Administrador",
                        Activo = true
                    });
                    await _context.SaveChangesAsync();

                    // ENVIAR CORREO DE CONFIRMACION DE CUENTA
                    var resultCorreo = _servicesEmail.ConfirmEmail("g.hector9983@gmail.com", user.Email, "rrmbjahggwhvkrgi", msg);
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
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // PASO 4 - METODO DE PAGO (A CONSULTAR)
            return RedirectToAction("Index", "Home");
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
