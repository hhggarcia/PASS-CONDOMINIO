using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoFacturaEmitida
{
    public int IdPagoFacturaEmitida { get; set; }

    public int IdFactura { get; set; }

    public int IdPagoRecibido { get; set; }

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;

    public virtual PagoRecibido IdPagoRecibidoNavigation { get; set; } = null!;
}
