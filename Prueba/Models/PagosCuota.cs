using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagosCuota
{
    public int? IdPago { get; set; }

    public int? IdRecibido { get; set; }

    public int? IdCuotaEspecial { get; set; }

    public int IdPagoRecibido { get; set; }
    public virtual ReciboCuota? IdRecibidoCuotaNavigation { get; set; }
    public virtual PagoRecibido? IdPagoCuotaNavigation { get; set; }

}
