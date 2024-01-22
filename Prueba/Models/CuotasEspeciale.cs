using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CuotasEspeciale
{
    public int IdCuotaEspecial { get; set; }

    public int IdCondominio { get; set; }

    public string Descripcion { get; set; } = null!;

    public int CantidadCuotas { get; set; }

    public decimal SubCuotas { get; set; }

    public decimal MontoMensual { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public bool Activa { get; set; }

    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReciboCuota> ReciboCuota { get; } = new List<ReciboCuota>();
}
