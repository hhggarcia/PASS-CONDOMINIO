using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CuentasPagar
{
    public int Id { get; set; }

    public int IdCondominio { get; set; }

    public int IdFactura { get; set; }

    /// <summary>
    /// Monto a pagar agregando retenciones
    /// </summary>
    public decimal Monto { get; set; }

    public string Status { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Factura IdFacturaNavigation { get; set; } = null!;
}
