using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CuentasCobrar
{
    public int Id { get; set; }

    public int IdCondominio { get; set; }

    public int IdFactura { get; set; }

    public decimal Monto { get; set; }

    public string Status { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;
}
