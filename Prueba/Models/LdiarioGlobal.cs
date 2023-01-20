using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Asiento")]
        public int IdAsiento { get; set; }

        [Display(Name = "Código de Cuenta")]
        public int IdCodCuenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = null!;
        public decimal Monto { get; set; }

        [Display(Name = "Tipo de Operación")]
        public bool TipoOperacion { get; set; }

        [Display(Name = "Número de Asiento")]
        public int NumAsiento { get; set; }

        [Display(Name = "# Codigo Cuenta")]
        public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
        public virtual ICollection<Activo> Activos { get; set; }
        public virtual ICollection<Gasto> Gastos { get; set; }
        public virtual ICollection<Ingreso> Ingresos { get; set; }
        public virtual ICollection<Pasivo> Pasivos { get; set; }
        public virtual ICollection<Patrimonio> Patrimonios { get; set; }
    }
}
