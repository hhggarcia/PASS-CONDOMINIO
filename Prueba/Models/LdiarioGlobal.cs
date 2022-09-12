using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class LdiarioGlobal
    {
        public LdiarioGlobal()
        {
            Gastos = new HashSet<Gasto>();
            Ingresos = new HashSet<Ingreso>();
        }

        public int IdAsiento { get; set; }
        public int IdCodCuenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Concepto { get; set; } = null!;
        public decimal Monto { get; set; }
        public bool TipoOperacion { get; set; }

        public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
        public virtual ICollection<Gasto> Gastos { get; set; }
        public virtual ICollection<Ingreso> Ingresos { get; set; }
    }
}
