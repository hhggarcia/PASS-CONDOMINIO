using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Clase
{
    public short Id { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; } = new List<CodigoCuentasGlobal>();

    public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
}
