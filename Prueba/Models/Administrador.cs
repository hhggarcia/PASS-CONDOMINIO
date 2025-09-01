using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Administrador
{
    public int IdAdministrador { get; set; }

    public int IdCondominio { get; set; }

    public string IdUsuario { get; set; } = string.Empty;

    public string Cargo { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;
}
