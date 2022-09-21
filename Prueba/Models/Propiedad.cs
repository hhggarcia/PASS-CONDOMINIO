using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Propiedad
    {
        public Propiedad()
        {
            PagoRecibidos = new HashSet<PagoRecibido>();
            PuestoEs = new HashSet<PuestoE>();
            ReciboCobros = new HashSet<ReciboCobro>();
        }

        public int IdPropiedad { get; set; }
        public int IdInmueble { get; set; }
        public string IdUsuario { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public decimal Dimensiones { get; set; }
        public decimal Alicuota { get; set; }
        public decimal Solvencia { get; set; }
        public decimal Saldo { get; set; }
        public decimal Deuda { get; set; }

        public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;
        public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;
        public virtual ICollection<PagoRecibido> PagoRecibidos { get; set; }
        public virtual ICollection<PuestoE> PuestoEs { get; set; }
        public virtual ICollection<ReciboCobro> ReciboCobros { get; set; }
    }
}
