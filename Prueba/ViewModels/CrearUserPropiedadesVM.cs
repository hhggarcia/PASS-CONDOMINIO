using Prueba.Models;
using System.ComponentModel.DataAnnotations;

namespace Prueba.ViewModels
{
    public class CrearUserPropiedadesVM
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

        public List<PropiedadVM> Propiedades { get; set; } = new List<PropiedadVM>();

        public List<GrupoVM> AvailableExpenseGroups { get; set; } = new List<GrupoVM>();
    }

    public class PropiedadVM
    {
        public string Codigo { get; set; } = string.Empty;
        public decimal Alicuota { get; set; }
        public decimal Saldo { get; set; }
        public decimal Deuda { get; set; }
        public List<GrupoVM> Grupos { get; set; } = new List<GrupoVM>();
    }

    public class GrupoVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Alicuota { get; set; }
    }
}
