using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Inquilino
{
    public int IdInquilino { get; set; }

    public string IdUsuario { get; set; } = string.Empty;

    public int IdPropiedad { get; set; }

    public string Rif { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Cedula { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;
}
