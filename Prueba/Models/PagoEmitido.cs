using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class PagoEmitido
    {
        public PagoEmitido()
        {
            ReferenciasPes = new HashSet<ReferenciasPe>();
        }

        public int IdPagoEmitido { get; set; }
        public int IdCondominio { get; set; }
        public int? IdProveedor { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public bool FormaPago { get; set; }

        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<ReferenciasPe> ReferenciasPes { get; set; }
    }
}
