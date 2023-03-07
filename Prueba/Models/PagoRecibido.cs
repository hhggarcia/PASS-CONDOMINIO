using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoRecibido
{
    public int IdPagoRecibido { get; set; }

    public int IdPropiedad { get; set; }

    public bool FormaPago { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int IdSubCuenta { get; set; }

    public string Concepto { get; set; } = null!;

    public bool Confirmado { get; set; }

    public decimal? ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual PagoReserva? PagoReserva { get; set; }

    public virtual ICollection<PagosRecibo> PagosRecibos { get; } = new List<PagosRecibo>();

    public virtual ICollection<ReferenciasPr> ReferenciasPrs { get; } = new List<ReferenciasPr>();
}
