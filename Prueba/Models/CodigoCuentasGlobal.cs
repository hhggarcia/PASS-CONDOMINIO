using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CodigoCuentasGlobal
{
    public int IdCodCuenta { get; set; }

    public int IdCondominio { get; set; }

    public int IdCodigo { get; set; }

    public virtual ICollection<Fondo> Fondos { get; } = new List<Fondo>();

    public virtual SubCuenta IdCodigoNavigation { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; } = new List<LdiarioGlobal>();

    public virtual ICollection<MonedaCuenta> MonedaCuenta { get; } = new List<MonedaCuenta>();

    public virtual ICollection<Provision> ProvisioneIdCodCuentaNavigations { get; } = new List<Provision>();

    public virtual ICollection<Provision> ProvisioneIdCodGastoNavigations { get; } = new List<Provision>();
}
