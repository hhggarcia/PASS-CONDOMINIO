using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class PagoRecibido
    {
        public PagoRecibido()
        {
            ReferenciasPrs = new HashSet<ReferenciasPr>();
        }

        public int IdPagoRecibido { get; set; }
        public int IdPropiedad { get; set; }
        public bool FormaPago { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public int IdSubCuenta { get; set; }
        public string Concepto { get; set; } = null!;

        public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
        public virtual ICollection<ReferenciasPr> ReferenciasPrs { get; set; }
    }
}
