using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagosRecibo
{
    public int? IdPago { get; set; }

    public int? IdRecibo { get; set; }

    public int IdPagoRecibo { get; set; }

    public virtual PagoRecibido IdPagoNavigation { get; set; } = null!;

    public virtual ReciboCobro IdReciboNavigation { get; set; } = null!;
}
