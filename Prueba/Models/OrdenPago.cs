using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class OrdenPago
{
    public int IdOrdenPago { get; set; }

    public int IdPagoEmitido { get; set; }

    public int IdProveedor { get; set; }

    public DateTime Fecha { get; set; }

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
