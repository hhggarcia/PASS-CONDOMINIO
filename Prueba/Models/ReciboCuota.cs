using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ReciboCuota
{
    public int IdReciboCuotas { get; set; }

    public int IdPropiedad { get; set; }

    public int IdCuotaEspecial { get; set; }

    public decimal SubCuotas { get; set; }

    public DateTime Fecha { get; set; }

    public bool EnProceso { get; set; }

    public bool Confirmado { get; set; }

    public int CuotasFaltantes { get; set; }

    public int CuotasPagadas { get; set; }

    public decimal Abonado { get; set; }

    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public virtual CuotasEspeciale IdCuotaEspecialNavigation { get; set; } = null!;

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual ICollection<PagosCuota> PagosCuota { get; set; } = new List<PagosCuota>();
}
