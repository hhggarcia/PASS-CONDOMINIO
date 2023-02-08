using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ReferenciaDolar
{
    public int IdReferencia { get; set; }

    public decimal Valor { get; set; }

    public DateTime Fecha { get; set; }

    public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; } = new List<LdiarioGlobal>();

    public virtual ICollection<PagoEmitido> PagoEmitidos { get; } = new List<PagoEmitido>();

    public virtual ICollection<PagoRecibido> PagoRecibidos { get; } = new List<PagoRecibido>();

    public virtual ICollection<ReciboCobro> ReciboCobros { get; } = new List<ReciboCobro>();

    public virtual ICollection<RelacionGasto> RelacionGastos { get; } = new List<RelacionGasto>();
}
