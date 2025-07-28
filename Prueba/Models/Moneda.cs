using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Moneda
{
    public int IdMoneda { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Pais { get; set; } = string.Empty;

    public string? Codigo { get; set; }

    public virtual ICollection<HistorialMoneda> HistorialMoneda { get; set; } = new List<HistorialMoneda>();

    public virtual ICollection<MonedaCond> MonedaConds { get; set; } = new List<MonedaCond>();
}
