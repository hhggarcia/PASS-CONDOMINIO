using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class SubCuenta
{
    public int Id { get; set; }

    public short IdCuenta { get; set; }

    public string Descricion { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public decimal? Saldo { get; set; }

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();

    public virtual Cuenta IdCuentaNavigation { get; set; } = new Cuenta();
}
