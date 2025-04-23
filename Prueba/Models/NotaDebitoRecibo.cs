using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaDebitoRecibo
{
    public int Id { get; set; }

    public int IdReciboCobro { get; set; }

    public int IdNotaDebito { get; set; }

    public virtual NotaDebito IdNotaDebitoNavigation { get; set; } = null!;

    public virtual ReciboCobro IdReciboCobroNavigation { get; set; } = null!;
}
