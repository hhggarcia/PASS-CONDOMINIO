using Prueba.Areas.Identity.Data;

namespace Prueba.Models
{
    public class Propiedad
    {
        public Propiedad()
        {
            PuestoEs = new HashSet<Puesto_Est>();
        }

        public int IdPropiedad { get; set; }
        public int IdInmueble { get; set; }
        public string IdUsuario { get; set; }
        public string Codigo { get; set; } = null!;
        public decimal Dimensiones { get; set; }
        public decimal Alicuota { get; set; }
        public bool Solvencia { get; set; }
        public int Saldo { get; set; }
        public int Deuda { get; set; }

        public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;
        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
        public virtual ICollection<Puesto_Est> PuestoEs { get; set; }
    }
}
