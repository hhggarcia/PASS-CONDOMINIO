using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaCreditosRecibo
{
    public int Id { get; set; }

    public int IdNotaCredito { get; set; }

    public int IdReciboCobro { get; set; }

    public virtual NotaCredito IdNotaCreditoNavigation { get; set; } = null!;

    public virtual ReciboCobro IdReciboCobroNavigation { get; set; } = null!;
}
