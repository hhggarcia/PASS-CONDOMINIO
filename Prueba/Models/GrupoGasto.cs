using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class GrupoGasto
{
    public int IdGrupoGasto { get; set; }

    public short NumGrupo { get; set; }

    public string NombreGrupo { get; set; } = null!;

    public virtual ICollection<CuentasGrupo> CuentasGrupos { get; } = new List<CuentasGrupo>();

    public virtual ICollection<PropiedadesGrupo> PropiedadesGrupos { get; } = new List<PropiedadesGrupo>();
}
