using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ComprobanteRetencionCliente
{
    public int IdComprobanteCliente { get; set; }

    public int? IdFactura { get; set; }

    public int IdCliente { get; set; }

    public DateTime FechaEmision { get; set; }

    public string Description { get; set; } = null!;

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

    public virtual ICollection<CompRetIslrFacturasEmitida> CompRetIslrFacturasEmitida { get; set; } = new List<CompRetIslrFacturasEmitida>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual FacturaEmitida? IdFacturaNavigation { get; set; }
}
