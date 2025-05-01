using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class HistorialMoneda
{
    public int IdHistorial { get; set; }

    public int IdMoneda { get; set; }

    public string BaseCode { get; set; } = null!;

    public string TargetCode { get; set; } = null!;

    public decimal ConversionRate { get; set; }

    public decimal ConversionResult { get; set; }

    public DateTime FechaConsulta { get; set; }

    public bool Actual { get; set; }

    public virtual Moneda IdMonedaNavigation { get; set; } = null!;
}
