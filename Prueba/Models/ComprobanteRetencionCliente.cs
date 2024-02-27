using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ComprobanteRetencionCliente
{
    public int IdComprobanteCliente { get; set; }

    public int IdFactura { get; set; }

    public int IdCliente { get; set; }

    public DateTime FechaEmision { get; set; }

    public string Description { get; set; } = null!;

    public decimal Retencion { get; set; }

    public decimal Sustraendo { get; set; }

    public decimal ValorRetencion { get; set; }

    public decimal TotalImpuesto { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;
}
