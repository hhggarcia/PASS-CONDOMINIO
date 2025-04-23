using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaCredito
{
    public int IdNotaCredito { get; set; }

    public string Concepto { get; set; } = null!;

    public string Comprobante { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public decimal Monto { get; set; }

    public int? IdPropiedad { get; set; }

    public int? IdPagoEmitido { get; set; }

    public string? Codigo { get; set; }

    public bool? Activo { get; set; }

    public virtual PagoEmitido? IdPagoEmitidoNavigation { get; set; }

    public virtual Propiedad? IdPropiedadNavigation { get; set; }

    public virtual ICollection<NotaCreditosRecibo> NotaCreditosRecibos { get; set; } = new List<NotaCreditosRecibo>();
}
