using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Estado
{
    public int IdEstado { get; set; }

    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual ICollection<Municipio> Municipios { get; } = new List<Municipio>();
}
