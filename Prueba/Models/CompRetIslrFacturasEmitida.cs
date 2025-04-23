using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CompRetIslrFacturasEmitida
{
    public int Id { get; set; }

    public int IdFactura { get; set; }

    public int IdCompRetIslr { get; set; }

    public virtual ComprobanteRetencionCliente IdCompRetIslrNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;
}
