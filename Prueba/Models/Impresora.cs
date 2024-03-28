using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Impresora
{
    public int IdImpresora { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdCondominio { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
}
