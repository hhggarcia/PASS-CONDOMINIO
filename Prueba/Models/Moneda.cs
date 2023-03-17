using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Moneda
{
    public int IdMoneda { get; set; }

    public string Nombre { get; set; } = null!;

    public string Pais { get; set; } = null!;

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();

    public virtual ICollection<MonedaCuenta> MonedaCuenta { get; } = new List<MonedaCuenta>();
}
