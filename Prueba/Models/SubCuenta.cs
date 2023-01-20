using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public partial class SubCuenta
    {
        public SubCuenta()
        {
            CodigoCuentasGlobals = new HashSet<CodigoCuentasGlobal>();
        }

        [Display(Name ="# Sub Cuenta")]
        public int Id { get; set; }

        [Display(Name = "Cuenta")]
        public short IdCuenta { get; set; }

        [Display(Name = "Descripción")]
        public string Descricion { get; set; } = null!;

        [Display(Name = "Código")]
        public string Codigo { get; set; } = null!;

        [Display(Name = "# Cuenta")]
        public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
        public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; }
    }
}
