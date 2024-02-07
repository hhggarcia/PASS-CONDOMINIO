using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Cuenta
{
    public short Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public short IdGrupo { get; set; }

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; } = new List<CodigoCuentasGlobal>();

    public virtual Grupo IdGrupoNavigation { get; set; } = null!;

    public virtual ICollection<SubCuenta> SubCuenta { get; set; } = new List<SubCuenta>();
    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();
    public virtual ICollection<SubCuenta> SubCuenta { get; } = new List<SubCuenta>();
    public virtual ICollection<Factura> Facturas { get; } = new List<Factura>();

}
