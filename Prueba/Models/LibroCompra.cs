using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class LibroCompra
{
    public int Id { get; set; }

    public int IdCondominio { get; set; }

    public int IdFactura { get; set; }

    public decimal BaseImponible { get; set; }

    public decimal ExentoIva { get; set; }

    public decimal Iva { get; set; }

    public decimal Igtf { get; set; }

    public decimal RetIva { get; set; }

    public decimal RetIslr { get; set; }

    public decimal Monto { get; set; }

    public int NumComprobanteRet { get; set; }

    public DateTime FechaComprobanteRet { get; set; }

    public bool FormaPago { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Factura IdFacturaNavigation { get; set; } = null!;
}
