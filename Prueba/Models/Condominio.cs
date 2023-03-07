using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Condominio
{
    public int IdCondominio { get; set; }

    public string IdAdministrador { get; set; } = null!;

    public string Rif { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();

    public virtual ICollection<EstadoResultado> EstadoResultados { get; } = new List<EstadoResultado>();

    public virtual AspNetUser IdAdministradorNavigation { get; set; } = null!;

    public virtual ICollection<Inmueble> Inmuebles { get; } = new List<Inmueble>();

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();

    public virtual ICollection<PagoEmitido> PagoEmitidos { get; } = new List<PagoEmitido>();

    public virtual ICollection<RelacionGasto> RelacionGastos { get; } = new List<RelacionGasto>();
}
