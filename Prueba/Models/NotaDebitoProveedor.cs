using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaDebitoProveedor
{
    public int IdNotaDebitoProveedor { get; set; }

    /// <summary>
    /// Relacion a factura de compra
    /// </summary>
    public int IdFactura { get; set; }

    public int? IdPagoEmitido { get; set; }

    public bool ExistPago { get; set; }

    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Monto a aumentar en la factura de compra
    /// </summary>
    public decimal Monto { get; set; }

    public string DetalleAjuste { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<CompRetIva> CompRetIvas { get; set; } = new List<CompRetIva>();

    public virtual ICollection<ComprobanteRetencion> ComprobanteRetencions { get; set; } = new List<ComprobanteRetencion>();

    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    public virtual PagoEmitido? IdPagoEmitidoNavigation { get; set; }

    public virtual ICollection<LibroCompra> LibroCompras { get; set; } = new List<LibroCompra>();
}
