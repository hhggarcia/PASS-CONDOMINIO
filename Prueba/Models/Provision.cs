using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Provision
{
    public int IdProvision { get; set; }

    public int IdCodGasto { get; set; }

    public int IdCodCuenta { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public decimal MontoRef { get; set; }

    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = string.Empty;

    public string SimboloRef { get; set; } = string.Empty;

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodGastoNavigation { get; set; } = null!;
}
