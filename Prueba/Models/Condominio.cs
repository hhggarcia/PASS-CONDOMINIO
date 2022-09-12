using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Condominio
    {
        public Condominio()
        {
            CodigoCuentasGlobals = new HashSet<CodigoCuentasGlobal>();
            PagoEmitidos = new HashSet<PagoEmitido>();
            RelacionGastos = new HashSet<RelacionGasto>();
        }

        public int IdCondominio { get; set; }
        public string IdAdministrador { get; set; } = null!;
        public int IdInmueble { get; set; }
        public string Rif { get; set; } = null!;
        public string Tipo { get; set; } = null!;

        public virtual AspNetUser IdAdministradorNavigation { get; set; } = null!;
        public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;
        public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; }
        public virtual ICollection<PagoEmitido> PagoEmitidos { get; set; }
        public virtual ICollection<RelacionGasto> RelacionGastos { get; set; }
    }
}
