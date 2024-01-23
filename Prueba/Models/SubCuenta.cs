using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class SubCuenta
{
    public int Id { get; set; }

    public string Descricion { get; set; } = null!;

    public string Codigo { get; set; } = null!;
    [Required]
    [Display(Name = "# Cuenta")]
    public short IdCuenta { get; set; }

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();
    [Display(Name = "Cuenta")]
    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
}
