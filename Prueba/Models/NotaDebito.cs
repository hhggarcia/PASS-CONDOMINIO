using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaDebito
{
    public int IdNotaDebito { get; set; }

    public string Codigo { get; set; } = null!;

    public string Concepto { get; set; } = null!;

    public bool Activo { get; set; }

    public int IdPropiedad { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public string? Comprobante { get; set; }

    public virtual ICollection<CompRetIva> CompRetIvas { get; set; } = new List<CompRetIva>();

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual ICollection<NotaDebitoRecibo> NotaDebitoRecibos { get; set; } = new List<NotaDebitoRecibo>();
}
