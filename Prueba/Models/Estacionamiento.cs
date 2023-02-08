using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public partial class Estacionamiento
    {
        public Estacionamiento()
        {
            PuestoEs = new HashSet<PuestoE>();
        }

        [Display(Name = "Estacionamiento")]
        public int IdEstacionamiento { get; set; }
        
        [Display(Name = "Inmueble")]
        public int IdInmueble { get; set; }
        public string Nombre { get; set; } = null!;
        
        [Display(Name = "Número de Puestos")]
        public int NumPuestos { get; set; }

        [Display(Name = "# Inmueble")]

        public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;
        public virtual ICollection<PuestoE> PuestoEs { get; set; }
    }
}
