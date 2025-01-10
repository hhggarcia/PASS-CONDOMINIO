using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PropiedadesGrupo
{
    public int IdPropiedadGrupo { get; set; }

    public int IdGrupoGasto { get; set; }

    public int IdPropiedad { get; set; }

    public decimal Alicuota { get; set; }

    public virtual GrupoGasto IdGrupoGastoNavigation { get; set; } = null!;

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
}
