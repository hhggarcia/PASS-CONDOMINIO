using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.ViewModels
{
    public class CrearFondoVM
    {
        public IList<SelectListItem>? Fondos { get; set; }
        public int IdFondo { get; set; }
        public int Porcentaje { get; set; }
    }
}
