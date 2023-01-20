using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public partial class RelacionGasto
    {
        public RelacionGasto()
        {
            ReciboCobros = new HashSet<ReciboCobro>();
        }

        [Display(Name = "# Relación Gastos")]
        public int IdRgastos { get; set; }

        [Display(Name ="Sub Total")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Total Mensual")]
        public decimal TotalMensual { get; set; }
        public DateTime Fecha { get; set; }

        [Display(Name = "Condominio")]
        public int IdCondominio { get; set; }

        [Display(Name = "# Condominio")]
        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<ReciboCobro> ReciboCobros { get; set; }
    }
}
