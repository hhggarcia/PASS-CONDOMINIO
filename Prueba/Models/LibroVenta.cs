using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class LibroVenta
{
    public int Id { get; set; }

    /// <summary>
    /// Condominio
    /// </summary>
    public int IdCondominio { get; set; }

    /// <summary>
    /// Factura
    /// </summary>
    public int IdFactura { get; set; }

    /// <summary>
    /// Monto SubTotal de la factura
    /// </summary>
    public decimal BaseImponible { get; set; }

    /// <summary>
    /// IVA de la factura
    /// </summary>
    public decimal Iva { get; set; }

    /// <summary>
    /// suma del iva y la base imponible
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Retencion de iva dependiendo del condominio ret
    /// </summary>
    public decimal RetIva { get; set; }

    /// <summary>
    /// Retencion islr dependiendo del condominio (tangibles)
    /// </summary>
    public decimal RetIslr { get; set; }

    /// <summary>
    /// Monto a cobrar
    /// </summary>
    public decimal Monto { get; set; }

    /// <summary>
    /// Numero de comprobacion de retencion
    /// </summary>
    public int NumComprobanteRet { get; set; }

    public bool FormaPago { get; set; }

    public decimal? TotalVentaIva { get; set; }

    public decimal? VentaExenta { get; set; }

    public decimal? VentaGravable { get; set; }

    public decimal? IvaRetenido { get; set; }

    public string? ComprobanteRetencion { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;

    public bool Activo { get; set; }
}
