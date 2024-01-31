using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Anticipo
{
    public int IdAnticipo { get; set; }

    public int Numero { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Saldo { get; set; }

    public string Detalle { get; set; } = null!;

    public int IdProveedor { get; set; }
    public bool Activo { get; set; }

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual ICollection<PagoAnticipo> PagoAnticipos { get; } = new List<PagoAnticipo>();
}
