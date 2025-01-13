using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CompRetIvaCliente
{
    public int IdComprobanteIva { get; set; }

    public int IdFactura { get; set; }

    public int IdCliente { get; set; }

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

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;

    public virtual NotaCredito? IdNotaCreditoNavigation { get; set; }

    public virtual NotaDebito? IdNotaDebitoNavigation { get; set; }
}
