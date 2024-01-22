using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class MonedaCuenta
{
    public int Id { get; set; }

    /// <summary>
    /// Codigo Sub cuenta del condominio
    /// </summary>
    public int IdCodCuenta { get; set; }

    /// <summary>
    /// Moneda asignada
    /// </summary>
    public int IdMoneda { get; set; }

    /// <summary>
    /// Mostrar cuenta en pago del propietario
    /// </summary>
    public bool RecibePagos { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal SaldoFinal { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual MonedaCond IdMonedaNavigation { get; set; } = null!;
}
