using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class RelacionGasto
    {
        public RelacionGasto()
        {
            ReciboCobros = new HashSet<ReciboCobro>();
        }

        public int IdRgastos { get; set; }
        public int? IdRegistroNomina { get; set; }
        public int? IdFactura { get; set; }
        public decimal GastosNomina { get; set; }
        public decimal GastosServicios { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GastosPatrimonio { get; set; }
        public decimal TotalMensual { get; set; }
        public DateTime Fecha { get; set; }
        public int IdCondominio { get; set; }

        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<ReciboCobro> ReciboCobros { get; set; }
    }
}
