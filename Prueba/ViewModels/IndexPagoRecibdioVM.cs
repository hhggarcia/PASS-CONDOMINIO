using Prueba.Areas.Identity.Data;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class IndexPagoRecibdioVM
    {
        public Dictionary<ApplicationUser, List<Propiedad>>? UsuariosPropiedad { get; set; }
        public Dictionary<Propiedad, List<PagoRecibido>>? PropiedadPagos { get; set; }
    }
}