using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoReserva
{
    public int IdPagoReserva { get; set; }

    public int IdReciboReserva { get; set; }

    public int IdPago { get; set; }

    public virtual PagoRecibido IdPagoReservaNavigation { get; set; } = null!;

    public virtual ReciboReserva IdReciboReservaNavigation { get; set; } = null!;
}
