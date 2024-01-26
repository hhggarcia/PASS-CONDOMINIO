using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Grupo
{
    public short Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Codigo { get; set; } = null!;
    public short IdClase { get; set; }
    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();
    public virtual ICollection<Cuenta> Cuenta { get; } = new List<Cuenta>();
    public virtual Clase IdClaseNavigation { get; set; } = new Clase();
}
