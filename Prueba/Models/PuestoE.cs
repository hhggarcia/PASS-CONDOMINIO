using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class PuestoE
    {
        public int IdPuestoE { get; set; }
        public int IdEstacionamiento { get; set; }
        public int IdPropiedad { get; set; }
        public string Codigo { get; set; } = null!;
        public int Alicuota { get; set; }

        public virtual Estacionamiento IdEstacionamientoNavigation { get; set; } = null!;
        public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
    }
}
