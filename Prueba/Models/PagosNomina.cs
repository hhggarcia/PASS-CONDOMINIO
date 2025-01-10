using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagosNomina
{
    public int Id { get; set; }

    public int IdReciboNomina { get; set; }

    public int IdPagoEmitido { get; set; }

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;

    public virtual ReciboNomina IdReciboNominaNavigation { get; set; } = null!;
}
