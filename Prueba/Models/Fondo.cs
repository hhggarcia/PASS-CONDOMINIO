using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Fondo
    {
        public int IdFondo { get; set; }
        public int IdCodCuenta { get; set; }
        public int Porcentaje { get; set; }

        public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
    }
}
