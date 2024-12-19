using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Prueba.ViewModels
{
    public class RegistrarInquilinoVM
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
        public int IdPropiedad { get; set; }
    }
}