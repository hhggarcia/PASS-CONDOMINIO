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

    public virtual ICollection<Deduccione> Deducciones { get; } = new List<Deduccione>();

    public virtual ICollection<Percepcione> Percepciones { get; } = new List<Percepcione>();

    public virtual ICollection<ReciboNomina> ReciboNominas { get; } = new List<ReciboNomina>();
}
