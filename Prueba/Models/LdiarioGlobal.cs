using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class LdiarioGlobal
{
    public int IdAsiento { get; set; }

    public int IdCodCuenta { get; set; }

    public DateTime Fecha { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Monto { get; set; }

    public bool TipoOperacion { get; set; }

    public int NumAsiento { get; set; }

    public decimal MontoRef { get; set; }

    public decimal ValorDolar { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual ICollection<Activo> Activos { get; } = new List<Activo>();

    public virtual ICollection<Gasto> Gastos { get; } = new List<Gasto>();

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual ICollection<Ingreso> Ingresos { get; } = new List<Ingreso>();

    public virtual ICollection<Pasivo> Pasivos { get; } = new List<Pasivo>();

    public virtual ICollection<Patrimonio> Patrimonios { get; } = new List<Patrimonio>();
}
