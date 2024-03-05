using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagosNotaDebito
{
    public int IdPagosNotaDebito { get; set; }

    public int IdPagoEmitido { get; set; }

    public int IdNotaDebito { get; set; }

    public virtual NotaDebito IdNotaDebitoNavigation { get; set; } = null!;

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
}
