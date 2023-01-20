using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public partial class PuestoE
    {
        [Display(Name = "Puesto")]
        public int IdPuestoE { get; set; }

        [Display(Name = "Estacionamiento")]
        public int IdEstacionamiento { get; set; }

        [Display(Name = "Propiedad")]
        public int IdPropiedad { get; set; }

        [Display(Name = "Código")]
        public string Codigo { get; set; } = null!;

        [Display(Name = "Alícuota")]
        public decimal Alicuota { get; set; }

        [Display(Name = "# Estacionamiento")]
        public virtual Estacionamiento IdEstacionamientoNavigation { get; set; } = null!;

        [Display(Name = "# Propiedad")]
        public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
    }
}
