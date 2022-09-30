using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Provision
    {
        public int IdProvision { get; set; }
        public int IdRgasto { get; set; }
        public int IdCodCuenta { get; set; }
        public decimal Monto { get; set; }

        public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
        public virtual RelacionGasto IdRgastoNavigation { get; set; } = null!;
    }
}
