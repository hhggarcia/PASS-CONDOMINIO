using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CompRetIvaFactura
{
    public int Id { get; set; }

    public int IdFactura { get; set; }

    public int IdCompRetIva { get; set; }

    public virtual CompRetIva IdCompRetIvaNavigation { get; set; } = null!;

    public virtual Factura IdFacturaNavigation { get; set; } = null!;
}
