using Prueba.Models;
using System.ComponentModel.DataAnnotations;

namespace Prueba.ViewModels
{
    public class CreateCondominioVM
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
}
