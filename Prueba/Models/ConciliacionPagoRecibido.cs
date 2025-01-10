using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ConciliacionPagoRecibido
{
    public int IdConciliacion { get; set; }

    public int IdPagoRecibido { get; set; }

    public int Id { get; set; }

    public virtual Conciliacion IdConciliacionNavigation { get; set; } = null!;

    public virtual PagoRecibido IdPagoRecibidoNavigation { get; set; } = null!;
}
