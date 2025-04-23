using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CompRetIvaFacturasEmitida
{
    public int Id { get; set; }

    public int IdFactura { get; set; }

    public int IdCompRetIva { get; set; }

    public virtual CompRetIvaCliente IdCompRetIvaNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;
}
