#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Services;
using Prueba.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Areas.Identity.Pages.Account
{
    public class NuevoCondominioModel : PageModel
    {
        private readonly NuevaAppContext _context;
        private readonly IEmailService _servicesEmail;
        private readonly UserManager<ApplicationUser> _userManager;


        public NuevoCondominioModel(NuevaAppContext context,
            IEmailService servicesEmail,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _servicesEmail = servicesEmail;
            _userManager = userManager;
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
            [StringLength(50, ErrorMessage = "The last name field should have a maximum of 50 characters")]
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
            public string Telefono { get; set; } = string.Empty;
            public string RifCondominio { get; set; } = null!;

            public string Tipo { get; set; } = null!;

            public string NombreCondominio { get; set; } = null!;

            public decimal InteresMora { get; set; }

            public string Direccion { get; set; } = null!;

            public string EmailCondominio { get; set; } = null!;

            public bool? ContribuyenteEspecial { get; set; }

            public decimal? Multa { get; set; }

            public string? ClaveCorreo { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }
            // PASO 1 - REGISTRO USUARIO SUPERADMIN
            // PASO 2 - REGISTRAR DATOS DE ADMINISTRADOR
            // PASO 3 - REGISTRAR DATOS DE CONDOMINIO
            // PASO 4 - METODO DE PAGO (A CONSULTAR)
            return Page();
        }
    }
}
