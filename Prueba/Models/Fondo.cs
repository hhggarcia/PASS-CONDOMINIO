using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Fondo
{
    public int IdFondo { get; set; }

    public int IdCodCuenta { get; set; }

    public int Porcentaje { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
}
