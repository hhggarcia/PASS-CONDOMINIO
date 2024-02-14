using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Provision
{
    public int IdProvision { get; set; }

    public int IdCodGasto { get; set; }

    public int IdCodCuenta { get; set; }

    public decimal Monto { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public decimal MontoRef { get; set; }

    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodGastoNavigation { get; set; } = null!;
}
