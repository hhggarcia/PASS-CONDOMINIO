using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ComprobanteRetencion
{
    public int IdComprobante { get; set; }

    public int? IdFactura { get; set; }

    public int IdProveedor { get; set; }

    public DateTime FechaEmision { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal Retencion { get; set; }

    public decimal Sustraendo { get; set; }

    public decimal ValorRetencion { get; set; }

    public decimal TotalImpuesto { get; set; }

    public string NumCompRet { get; set; } = null!;

    public int NumComprobante { get; set; }

    public decimal TotalFactura { get; set; }

    public decimal BaseImponible { get; set; }

    public bool? IsFactura { get; set; }

    public int? IdNotaCredito { get; set; }

    public bool? IsNotaCredito { get; set; }

    public int? IdNotaDebito { get; set; }

    public bool? IsNotaDebito { get; set; }

    public bool? Activo { get; set; }

    public bool IsMultiFactura { get; set; }

    public virtual ICollection<CompRetIslrFactura> CompRetIslrFacturas { get; set; } = new List<CompRetIslrFactura>();

    public virtual Factura? IdFacturaNavigation { get; set; }

    public virtual NotaCreditoProveedor? IdNotaCreditoNavigation { get; set; }

    public virtual NotaDebitoProveedor? IdNotaDebitoNavigation { get; set; }

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
