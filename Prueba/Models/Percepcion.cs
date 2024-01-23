using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Percepcion
{
    public int IdPercepcion { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Monto { get; set; }

    public decimal RefMonto { get; set; }

    public bool Activo { get; set; }

    public int IdEmpleado { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
