using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoAnticipo
{
    public int IdPagoEmitido { get; set; }

    public int IdAnticipo { get; set; }

    public int Id { get; set; }

    public virtual Anticipo IdAnticipoNavigation { get; set; } = null!;

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
}
