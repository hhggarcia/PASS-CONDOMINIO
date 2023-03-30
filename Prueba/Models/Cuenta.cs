using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Cuenta
{
    public short Id { get; set; }

    public short IdGrupo { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public virtual Grupo IdGrupoNavigation { get; set; } = new Grupo();

    public virtual ICollection<SubCuenta> SubCuenta { get; } = new List<SubCuenta>();
}
