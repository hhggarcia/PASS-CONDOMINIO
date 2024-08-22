using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ConciliacionPagoEmitido
{
    public int IdConciliacion { get; set; }

    public int IdPagoEmitido { get; set; }

    public int Id { get; set; }

    public virtual Conciliacion IdConciliacionNavigation { get; set; } = null!;

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
}
