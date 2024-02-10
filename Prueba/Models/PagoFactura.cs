using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoFactura
{
    public int Id { get; set; }

    public int IdFactura { get; set; }

    public int IdPagoEmitido { get; set; }
    public int IdAnticipo { get; set; }
    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
    public virtual Anticipo IdAnticipoNavigation { get; set; } = null!;
}
