using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class MonedaCond
{
    public int IdMonedaCond { get; set; }

    public int IdCondominio { get; set; }

    public int IdMoneda { get; set; }

    public bool Princinpal { get; set; }

    public string Simbolo { get; set; } = null!;

    /// <summary>
    /// Valor de la moneda respecto al dolar
    /// </summary>
    public decimal ValorDolar { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Moneda IdMonedaNavigation { get; set; } = null!;
}
