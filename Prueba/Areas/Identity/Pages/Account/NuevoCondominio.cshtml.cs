#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Services;
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


        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Nombre(s)")]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Apellido(s)")]
            public string Surname { get; set; }

            //Cambiar a Custom, para contener solo numeros y puntos
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Cédula")]
            public string Cedula { get; set; }
        }
    }
}
