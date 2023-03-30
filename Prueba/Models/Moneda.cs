using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Moneda
{
    public int IdMoneda { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Pais { get; set; } = string.Empty;

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();
}
