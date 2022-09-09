using Prueba.Areas.Identity.Data;

namespace Prueba.Models
{
    public class Condominio
    {
        public int IdCondominio { get; set; }
        public string? IdUsuario { get; set; }
        public int IdInmueble { get; set; }
        public int Rif { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
        public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;
    }
}
