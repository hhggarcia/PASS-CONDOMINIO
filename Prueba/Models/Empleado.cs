using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Empleado
{
    public int IdEmpleado { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public int Cedula { get; set; }

    public DateTime FechaIngreso { get; set; }

    public bool Estado { get; set; }

    public decimal BaseSueldo { get; set; }

    public decimal RefMonto { get; set; }

    public virtual ICollection<Bonificacion> Bonificaciones { get; set; } = new List<Bonificacion>();

    public virtual ICollection<CondominioNomina> CondominioNominas { get; set; } = new List<CondominioNomina>();

    public virtual ICollection<Deduccion> Deducciones { get; set; } = new List<Deduccion>();

    public virtual ICollection<Percepcion> Percepciones { get; set; } = new List<Percepcion>();

    public virtual ICollection<ReciboNomina> ReciboNominas { get; set; } = new List<ReciboNomina>();
}
