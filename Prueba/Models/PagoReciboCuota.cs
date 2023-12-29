using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoReciboCuota
{
    public int IdPagoRecibido { get; set; }

    public int? IdPropiedad { get; set; }

    public int? IdCuota { get; set; }

    public bool? FormaPago { get; set; }

    public decimal? Monto { get; set; }

    public DateTime? Fecha { get; set; }

    public int? IdSubCuenta { get; set; }

    public string? Concepto { get; set; }

    public bool? Confirmado { get; set; }

    public decimal? ValorDolar { get; set; }

    public decimal? MontoRef { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual CuotasEspeciale? IdCuotaNavigation { get; set; }

    public virtual ICollection<PagosCuotasRecibido> PagosCuotasRecibidos { get; } = new List<PagosCuotasRecibido>();
}
