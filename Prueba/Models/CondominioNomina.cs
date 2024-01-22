using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CondominioNomina
{
    public int IdCondominio { get; set; }

    public int IdReciboNomina { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ReciboNomina IdReciboNominaNavigation { get; set; } = null!;
}
