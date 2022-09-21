using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class SubCuenta
    {
        public SubCuenta()
        {
            CodigoCuentasGlobals = new HashSet<CodigoCuentasGlobal>();
        }

        public int Id { get; set; }
        public short IdCuenta { get; set; }
        public string Descricion { get; set; } = null!;
        public string Codigo { get; set; } = null!;

        public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
        public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; }
    }
}
