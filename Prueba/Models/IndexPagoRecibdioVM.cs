using Prueba.Areas.Identity.Data;

namespace Prueba.Models
{
    public class IndexPagoRecibdioVM
    {
        public Dictionary<ApplicationUser, List<Propiedad>>? UsuariosPropiedad { get; set; }
        public Dictionary<Propiedad, List<PagoRecibido>>? PropiedadPagos { get; set; }
        public IList<ReferenciaDolar>? ReferenciasDolar { get; set; } 
    }
}
