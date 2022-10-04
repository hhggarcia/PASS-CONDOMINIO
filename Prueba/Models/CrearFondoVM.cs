using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class CrearFondoVM
    {
        public IList<SelectListItem>? Fondos { get; set; }
        public int IdFondo { get; set; }
        public int Porcentaje { get; set; }
    }
}
