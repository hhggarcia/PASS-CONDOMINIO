using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Clase
{
    public short Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();
}
