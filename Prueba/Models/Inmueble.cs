using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Inmueble
{
    public int IdInmueble { get; set; }

    public string Ubicacion { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int TotalPropiedad { get; set; }

    public int IdCondominio { get; set; }

    public virtual ICollection<AreaComun> AreaComuns { get; } = new List<AreaComun>();

    public virtual ICollection<Estacionamiento> Estacionamientos { get; } = new List<Estacionamiento>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<Propiedad> Propiedads { get; } = new List<Propiedad>();
}
