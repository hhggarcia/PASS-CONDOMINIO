using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaDebitoCliente
{
    public int IdNotaDebitoCliente { get; set; }

    /// <summary>
    /// Relacion a factura de venta
    /// </summary>
    public int IdFactura { get; set; }

    public int? IdPagoRecibido { get; set; }

    public bool ExistPago { get; set; }

    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Monto a aumentar en la factura de venta
    /// </summary>
    public decimal Monto { get; set; }

    public string DetalleIncremento { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<CompRetIvaCliente> CompRetIvaClientes { get; set; } = new List<CompRetIvaCliente>();

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;

    public virtual PagoRecibido? IdPagoRecibidoNavigation { get; set; }

    public virtual ICollection<LibroVenta> LibroVenta { get; set; } = new List<LibroVenta>();
}
