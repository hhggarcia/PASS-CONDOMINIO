using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class EstadoResultado
{
    public int IdEstResultado { get; set; }

    public decimal TotalIngresos { get; set; }

    public decimal TotalGastos { get; set; }

    public DateOnly Fecha { get; set; }

    public int IdCondominio { get; set; }

    public decimal? MontoRef { get; set; }

    public decimal? ValorDolar { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
}
