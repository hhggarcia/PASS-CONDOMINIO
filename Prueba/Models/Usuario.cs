using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public class Usuario
    {
        [Required]

        public string? FirstName { get; set; } = string.Empty;
        [Required]

        public string? LastName { get; set; } = string.Empty;
        [Required]

        public string? Email { get; set; } = string.Empty;
        [Required]
        public string? Password { get; set; } = string.Empty;
        //public string? CantPropiedades { get; set; }
    }
}
