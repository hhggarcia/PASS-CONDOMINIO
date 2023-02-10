using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Pais
{
    public int IdPais { get; set; }

    public string Abreviatura { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Estado> Estados { get; } = new List<Estado>();
}
