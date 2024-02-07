using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class ReciboNomina
{
    public int IdReciboNomina { get; set; }

    public int IdCondominio { get; set; }

    public bool Entregado { get; set; }

    public int IdEmpleado { get; set; }

    public DateTime Fecha { get; set; }

    public bool Periodo { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal PagoTotal { get; set; }

    public decimal RefMonto { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual ICollection<PagosNomina> PagosNominas { get; set; } = new List<PagosNomina>();
}
