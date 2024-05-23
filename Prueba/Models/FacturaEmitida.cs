using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class FacturaEmitida
{
    public int IdFacturaEmitida { get; set; }

    public int IdProducto { get; set; }

    public int NumFactura { get; set; }

    public string NumControl { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Iva { get; set; }

    public decimal MontoTotal { get; set; }

    public decimal Abonado { get; set; }

    public bool Pagada { get; set; }

    public bool EnProceso { get; set; }

    public int IdCodCuenta { get; set; }

    public int IdCliente { get; set; }

    public virtual ICollection<CompRetIvaCliente> CompRetIvaClientes { get; set; } = new List<CompRetIvaCliente>();

    public virtual ICollection<ComprobanteRetencionCliente> ComprobanteRetencionClientes { get; set; } = new List<ComprobanteRetencionCliente>();

    public virtual ICollection<CuentasCobrar> CuentasCobrars { get; set; } = new List<CuentasCobrar>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual ICollection<LibroVenta> LibroVenta { get; set; } = new List<LibroVenta>();

    public virtual ICollection<NotaCredito> NotaCreditos { get; set; } = new List<NotaCredito>();

    public virtual ICollection<PagoFacturaEmitida> PagoFacturaEmitida { get; set; } = new List<PagoFacturaEmitida>();
}
