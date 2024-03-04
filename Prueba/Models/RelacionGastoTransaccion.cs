using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class RelacionGastoTransaccion
{
    public int Id { get; set; }

    public int IdRelacionGasto { get; set; }

    public int IdTransaccion { get; set; }

    public virtual RelacionGasto IdRelacionGastoNavigation { get; set; } = null!;

    public virtual Transaccion IdTransaccionNavigation { get; set; } = null!;
}
