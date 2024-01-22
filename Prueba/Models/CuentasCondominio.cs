using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CuentasCondominio
{
    public int IdCondominio { get; set; }

    public int IdCodCuenta { get; set; }

    public int Id { get; set; }

    public int NumGrupo { get; set; }

    public string NombreGrupo { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
}
