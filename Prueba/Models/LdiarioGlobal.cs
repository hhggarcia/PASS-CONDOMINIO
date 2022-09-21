using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class LdiarioGlobal
    {
        public LdiarioGlobal()
        {
            Activos = new HashSet<Activo>();
            Gastos = new HashSet<Gasto>();
            Ingresos = new HashSet<Ingreso>();
            Pasivos = new HashSet<Pasivo>();
            Patrimonios = new HashSet<Patrimonio>();
        }

        public int IdAsiento { get; set; }
        public int IdCodCuenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Concepto { get; set; } = null!;
        public decimal Monto { get; set; }
        public bool TipoOperacion { get; set; }

        public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
        public virtual ICollection<Activo> Activos { get; set; }
        public virtual ICollection<Gasto> Gastos { get; set; }
        public virtual ICollection<Ingreso> Ingresos { get; set; }
        public virtual ICollection<Pasivo> Pasivos { get; set; }
        public virtual ICollection<Patrimonio> Patrimonios { get; set; }
    }
}
