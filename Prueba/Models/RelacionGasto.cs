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
        public decimal SubTotal { get; set; }
        public decimal TotalMensual { get; set; }
        public DateTime Fecha { get; set; }
        public int IdCondominio { get; set; }

        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<ReciboCobro> ReciboCobros { get; set; }
    }
}
