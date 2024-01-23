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

    public virtual ICollection<CuentasCobrar> CuentasCobrars { get; } = new List<CuentasCobrar>();

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual ICollection<LibroVenta> LibroVenta { get; } = new List<LibroVenta>();
}
