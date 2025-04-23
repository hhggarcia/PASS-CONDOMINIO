using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaCreditoCliente
{
    public int IdNotaCreditoCliente { get; set; }

    /// <summary>
    /// Relacion a factura de venta
    /// </summary>
    public int IdFactura { get; set; }

    public int? IdPagoEmitido { get; set; }

    public bool ExistPago { get; set; }

    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Monto a reducir en la factura de venta
    /// </summary>
    public decimal Monto { get; set; }

    public string MotivoAjuste { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<CompRetIvaCliente> CompRetIvaClientes { get; set; } = new List<CompRetIvaCliente>();

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;

    public virtual PagoEmitido? IdPagoEmitidoNavigation { get; set; }

    public virtual ICollection<LibroVenta> LibroVenta { get; set; } = new List<LibroVenta>();
}
