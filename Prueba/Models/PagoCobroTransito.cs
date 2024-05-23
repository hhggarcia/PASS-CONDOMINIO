using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoCobroTransito
{
    public int IdPagoCobroTransito { get; set; }

    public int IdPagoRecibido { get; set; }

    public int IdCobroTransito { get; set; }

    public virtual CobroTransito IdCobroTransitoNavigation { get; set; } = null!;

    public virtual PagoRecibido IdPagoRecibidoNavigation { get; set; } = null!;
}
