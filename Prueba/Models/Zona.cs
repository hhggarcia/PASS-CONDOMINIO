using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Zona
    {
        public int IdZona { get; set; }
        public int IdParroquia { get; set; }
        public string Zona1 { get; set; } = null!;
        public int CodigoPostal { get; set; }

        public virtual Parroquia IdParroquiaNavigation { get; set; } = null!;
        public virtual Inmueble Inmueble { get; set; } = null!;
    }
}
