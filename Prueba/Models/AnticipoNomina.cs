using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class AnticipoNomina
{
    public int IdAnticipoNomina { get; set; }

    public int IdEmpleado { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public bool Activo { get; set; }

    public int? IdPagoEmitido { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual PagoEmitido? IdPagoEmitidoNavigation { get; set; }
}
