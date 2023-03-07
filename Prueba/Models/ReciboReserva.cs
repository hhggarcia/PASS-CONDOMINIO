using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ReciboReserva
{
    public int IdReciboReserva { get; set; }

    public int IdReserva { get; set; }

    public bool Pagado { get; set; }

    public bool EnProceso { get; set; }

    public decimal Monto { get; set; }

    public decimal Abonado { get; set; }

    public decimal MontoRef { get; set; }

    public decimal? ValorDolar { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Reserva IdReservaNavigation { get; set; } = null!;

    public virtual ICollection<PagoReserva> PagoReservas { get; } = new List<PagoReserva>();
}
