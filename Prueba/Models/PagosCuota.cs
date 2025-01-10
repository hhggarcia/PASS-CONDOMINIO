using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagosCuota
{
    public int IdPagoCuota { get; set; }

    public int IdReciboCuota { get; set; }

    public int IdPagoRecibido { get; set; }

    public virtual PagoRecibido IdPagoRecibidoNavigation { get; set; } = null!;

    public virtual ReciboCuota IdReciboCuotaNavigation { get; set; } = null!;
}
