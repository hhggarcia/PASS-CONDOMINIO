using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Bonificacion
{
    public int IdBonificacion { get; set; }

    public int IdEmpleado { get; set; }

    public int IdCodCuenta { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Monto { get; set; }

    public decimal RefMonto { get; set; }

    public bool Activo { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
