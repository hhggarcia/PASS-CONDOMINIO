using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Moneda
{
    public int IdMoneda { get; set; }

    public string Nombre { get; set; } = null!;

    public string Pais { get; set; } = null!;

    public string Simbolo { get; set; } = null!;

    /// <summary>
    /// Valor de la moneda respecto al dolar
    /// </summary>
    public decimal ValorDolar { get; set; }

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();
}
