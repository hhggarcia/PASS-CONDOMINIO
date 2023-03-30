using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Grupo
{
    public short Id { get; set; }

    public short IdClase { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public virtual ICollection<Cuenta> Cuenta { get; } = new List<Cuenta>();

    public virtual Clase IdClaseNavigation { get; set; } = new Clase();
}
