using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class SubCuenta
{
    public int Id { get; set; }

    public string Descricion { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public short IdCuenta { get; set; }

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; } = new List<CodigoCuentasGlobal>();

    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
}
