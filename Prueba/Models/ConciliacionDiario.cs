using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ConciliacionDiario
{
    public int IdConciliacion { get; set; }

    public int IdDiario { get; set; }

    public virtual Conciliacion IdConciliacionNavigation { get; set; } = null!;

    public virtual LdiarioGlobal IdDiarioNavigation { get; set; } = null!;
}
