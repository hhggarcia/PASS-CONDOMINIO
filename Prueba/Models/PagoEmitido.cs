using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public partial class PagoEmitido
    {
        public PagoEmitido()
        {
            ReferenciasPes = new HashSet<ReferenciasPe>();
        }

        [Display(Name = "# Pago Emitido")]
        public int IdPagoEmitido { get; set; }

        [Display(Name = "Condominio")]
        public int IdCondominio { get; set; }

        [Display(Name = "Proveedor")]
        public int? IdProveedor { get; set; }

        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }

        [Display(Name = "Forma de Pago")]
        public bool FormaPago { get; set; }

        [Display(Name = "# Condominio")]
        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<ReferenciasPe> ReferenciasPes { get; set; }
    }
}
