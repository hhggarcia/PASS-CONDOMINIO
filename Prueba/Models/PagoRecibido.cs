using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoRecibido
{
    public int IdPagoRecibido { get; set; }

    public int IdPropiedad { get; set; }

    public bool FormaPago { get; set; }

    public decimal Monto { get; set; }

    public DateOnly Fecha { get; set; }

    public int IdSubCuenta { get; set; }

    public string Concepto { get; set; } = null!;

    public bool Confirmado { get; set; }

    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual PagoReserva? PagoReserva { get; set; }

    public virtual ICollection<PagosCuota> PagosCuota { get; set; } = new List<PagosCuota>();

    public virtual ICollection<PagosRecibo> PagosRecibos { get; set; } = new List<PagosRecibo>();

    public virtual ICollection<ReferenciasPr> ReferenciasPrs { get; set; } = new List<ReferenciasPr>();
}
