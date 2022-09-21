using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class CodigoCuentasGlobal
    {
        public CodigoCuentasGlobal()
        {
            LdiarioGlobals = new HashSet<LdiarioGlobal>();
        }

        public int IdCodCuenta { get; set; }
        public int IdCondominio { get; set; }
        public int IdCodigo { get; set; }

        public virtual SubCuenta IdCodigoNavigation { get; set; } = null!;
        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; set; }
    }
}
