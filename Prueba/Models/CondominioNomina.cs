using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CondominioNomina
{
    public int IdCondominio { get; set; }

    public int IdEmpleado { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
