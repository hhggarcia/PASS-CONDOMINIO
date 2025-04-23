using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CompRetIva
{
    public int IdComprobanteIva { get; set; }

    public int? IdFactura { get; set; }

    public int IdProveedor { get; set; }

    public DateTime FechaEmision { get; set; }

    public int? IdNotaCredito { get; set; }

    public int? IdNotaDebito { get; set; }

    public bool TipoTransaccion { get; set; }

    public string NumFacturaAfectada { get; set; } = null!;

    public decimal TotalCompraIva { get; set; }

    public decimal CompraSinCreditoIva { get; set; }

    public decimal BaseImponible { get; set; }

    public decimal Alicuota { get; set; }

    public decimal ImpIva { get; set; }

    public decimal IvaRetenido { get; set; }

    public decimal TotalCompraRetIva { get; set; }

    public string NumCompRet { get; set; } = null!;

    public int NumComprobante { get; set; }

    public bool? IsFactura { get; set; }

    public bool? IsNotaCredito { get; set; }

    public bool? IsNotaDebito { get; set; }

    public bool? Activo { get; set; }

    public bool IsMultiFactura { get; set; }

    public virtual ICollection<CompRetIvaFactura> CompRetIvaFacturas { get; set; } = new List<CompRetIvaFactura>();

    public virtual Factura? IdFacturaNavigation { get; set; }

    public virtual NotaCreditoProveedor? IdNotaCreditoNavigation { get; set; }

    public virtual NotaDebitoProveedor? IdNotaDebito1 { get; set; }

    public virtual NotaDebito? IdNotaDebitoNavigation { get; set; }

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
