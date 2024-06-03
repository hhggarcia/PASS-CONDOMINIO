using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class RelacionGasto
{
    public int IdRgastos { get; set; }

    public decimal SubTotal { get; set; }

    public decimal TotalMensual { get; set; }

    public DateTime Fecha { get; set; }

    public int IdCondominio { get; set; }

    public decimal MontoRef { get; set; }

    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public string Mes { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReciboCobro> ReciboCobros { get; set; } = new List<ReciboCobro>();

    public virtual ICollection<RelacionGastoTransaccion> RelacionGastoTransaccions { get; set; } = new List<RelacionGastoTransaccion>();
}
