using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Factura
{
    public int IdFactura { get; set; }

    public int IdCondominio { get; set; }

    public int IdProveedor { get; set; }

    public string RazonSocial { get; set; } = null!;

    public int NumFactura { get; set; }

    public decimal Total { get; set; }

    public DateTime Fecha { get; set; }
}
