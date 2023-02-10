using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class AreaComun
{
    public int IdArea { get; set; }

    public int IdInmueble { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; } = new List<Reserva>();
}
