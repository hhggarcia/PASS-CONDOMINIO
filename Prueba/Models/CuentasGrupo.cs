using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CuentasGrupo
{
    public int IdCuentaGrupos { get; set; }

    public int IdCodCuenta { get; set; }

    public int IdGrupoGasto { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual GrupoGasto IdGrupoGastoNavigation { get; set; } = null!;
}
