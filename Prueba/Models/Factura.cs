using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Factura
{
    public int IdFactura { get; set; }

    public int NumFactura { get; set; }

    public string NumControl { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Iva { get; set; }

    public decimal MontoTotal { get; set; }

    public int IdProveedor { get; set; }

    public decimal Abonado { get; set; }

    public bool Pagada { get; set; }

    public bool EnProceso { get; set; }
    public short IdCodCuenta { get; set; }

    public virtual ICollection<CuentasPagar> CuentasPagars { get; } = new List<CuentasPagar>();

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
    public virtual Cuenta IdCodCuentaNavigation { get; set; } = null!;

    public virtual ICollection<LibroCompra> LibroCompras { get; } = new List<LibroCompra>();

    public virtual ICollection<PagoFactura> PagoFacturas { get; } = new List<PagoFactura>();

}
