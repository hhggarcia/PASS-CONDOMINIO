using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Inmueble
    {
        public Inmueble()
        {
            Estacionamientos = new HashSet<Estacionamiento>();
            Propiedads = new HashSet<Propiedad>();
        }

        public int IdInmueble { get; set; }
        public int IdZona { get; set; }
        public string Nombre { get; set; } = null!;
        public int TotalPropiedad { get; set; }
        public int IdCondominio { get; set; }

        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual Zona IdInmuebleNavigation { get; set; } = null!;
        public virtual ICollection<Estacionamiento> Estacionamientos { get; set; }
        public virtual ICollection<Propiedad> Propiedads { get; set; }
    }
}
