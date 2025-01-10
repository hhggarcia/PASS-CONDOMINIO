using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class EstadoSituacion
{
    public int IdEstSituacion { get; set; }

    public decimal TotalAct { get; set; }

    public decimal TotalPas { get; set; }

    public decimal TotalPat { get; set; }

    public DateOnly Fecha { get; set; }

    public int IdCondominio { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
}
