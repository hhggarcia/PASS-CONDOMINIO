using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Estacionamiento
{
    public int IdEstacionamiento { get; set; }

    public int IdInmueble { get; set; }

    public string Nombre { get; set; } = null!;

    public int NumPuestos { get; set; }

    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual ICollection<PuestoE> PuestoEs { get; } = new List<PuestoE>();
}
