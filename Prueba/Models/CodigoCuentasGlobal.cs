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
        public short Clase { get; set; }
        public short Grupo { get; set; }
        public short Cuenta { get; set; }
        public short Subcuenta { get; set; }
        public string Description { get; set; } = null!;
        public int IdCondominio { get; set; }

        public virtual Condominio IdCondominioNavigation { get; set; } = null!;
        public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; set; }
    }
}
