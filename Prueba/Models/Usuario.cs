using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public class Usuario
    {
        [Required]
        [StringLength(255, ErrorMessage = "Máximo 255 carácteres")]
        [Display(Name = "Nombre")]

        public string? FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(255, ErrorMessage = "Máximo 255 carácteres")]
        [Display(Name = "Apellido")]
        public string? LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [Display(Name = "Correo")]

        public string? Email { get; set; } = string.Empty;
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
