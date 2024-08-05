using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Conciliacion
{
    public int IdConciliacion { get; set; }

    public int IdCondominio { get; set; }

    public int IdCodCuenta { get; set; }

    public DateTime FechaEmision { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal SaldoFinal { get; set; }

    public bool Actual { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public decimal TotalIngreso { get; set; }

    public decimal TotalEgreso { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
}
