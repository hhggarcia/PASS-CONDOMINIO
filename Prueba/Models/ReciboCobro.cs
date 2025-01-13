using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ReciboCobro
{
    public int IdReciboCobro { get; set; }

    public int IdPropiedad { get; set; }

    public int IdRgastos { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public bool Pagado { get; set; }

    public bool EnProceso { get; set; }

    public decimal Abonado { get; set; }

    public decimal MontoRef { get; set; }

    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public decimal MontoMora { get; set; }

    public decimal MontoIndexacion { get; set; }

    public decimal Acumulado { get; set; }

    public string Mes { get; set; } = null!;

    public bool ReciboActual { get; set; }

    public decimal TotalPagar { get; set; }

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual RelacionGasto IdRgastosNavigation { get; set; } = null!;

    public virtual ICollection<PagosRecibo> PagosRecibos { get; set; } = new List<PagosRecibo>();
}
