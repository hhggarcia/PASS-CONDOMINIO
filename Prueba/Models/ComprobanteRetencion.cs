using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ComprobanteRetencion
{
    public int IdComprobante { get; set; }

    public int IdFactura { get; set; }

    public int IdProveedor { get; set; }

    public DateTime FechaEmision { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal Retencion { get; set; }

    public decimal Sustraendo { get; set; }

    public decimal ValorRetencion { get; set; }

    public decimal TotalImpuesto { get; set; }

    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
