using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Inquilino
{
    public int IdInquilino { get; set; }

    public string IdUsuario { get; set; } = null!;

    public int IdPropiedad { get; set; }

    public string Rif { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Cedula { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;
}
