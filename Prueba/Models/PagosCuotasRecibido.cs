using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagosCuotasRecibido
{
    public int? IdPago { get; set; }

    public int? IdRecibido { get; set; }

    public int? IdCuotaEspecial { get; set; }

    public int IdPagoRecibido { get; set; }

    public virtual PagoReciboCuota? IdPagoNavigation { get; set; }

    public virtual ReciboCuota? IdRecibidoNavigation { get; set; }
}
