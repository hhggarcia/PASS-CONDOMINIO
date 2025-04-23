using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CompRetIslrFactura
{
    public int Id { get; set; }

    public int IdFactura { get; set; }

    public int IdCompRetIslr { get; set; }

    public virtual ComprobanteRetencion IdCompRetIslrNavigation { get; set; } = null!;

    public virtual Factura IdFacturaNavigation { get; set; } = null!;
}
