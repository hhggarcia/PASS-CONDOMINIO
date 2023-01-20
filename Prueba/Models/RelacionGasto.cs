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

    public int IdDolar { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ReferenciaDolar IdDolarNavigation { get; set; } = null!;

    public virtual ICollection<ReciboCobro> ReciboCobros { get; } = new List<ReciboCobro>();
}
