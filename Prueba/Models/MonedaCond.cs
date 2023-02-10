using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class MonedaCond
{
    public int IdMonedaCond { get; set; }

    public int IdCondominio { get; set; }

    public int IdMoneda { get; set; }

    public bool Princinpal { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Moneda IdMonedaNavigation { get; set; } = null!;

    public virtual PagoEmitido? PagoEmitido { get; set; }

    public virtual ICollection<PagoRecibido> PagoRecibidos { get; } = new List<PagoRecibido>();
}
