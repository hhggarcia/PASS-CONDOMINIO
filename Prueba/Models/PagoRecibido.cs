using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class PagoRecibido
    {
        public int IdPagoRecibido { get; set; }
        public int IdPropiedad { get; set; }
        public bool FormaPago { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public bool Conciliado { get; set; }

        public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
        public virtual ReferenciasPr ReferenciasPr { get; set; } = null!;
    }
}
