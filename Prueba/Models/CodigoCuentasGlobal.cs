using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class CodigoCuentasGlobal
    {
        public CodigoCuentasGlobal()
        {
            Fondos = new HashSet<Fondo>();
            LdiarioGlobals = new HashSet<LdiarioGlobal>();
            Provisiones = new HashSet<Provision>();
        }

        public int IdCodCuenta { get; set; }
        public int IdCondominio { get; set; }
        public int IdCodigo { get; set; }

        public virtual SubCuenta IdCodigoNavigation { get; set; } = null!;
        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<Fondo> Fondos { get; set; }
        public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; set; }
        public virtual ICollection<Provision> Provisiones { get; set; }
    }
}
