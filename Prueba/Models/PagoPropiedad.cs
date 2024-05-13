using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoPropiedad
{
    public int IdPagoPropiedad { get; set; }

    public int IdPago { get; set; }

    public int IdPropiedad { get; set; }

    public bool Confirmado { get; set; }

    public bool Rectificado { get; set; }

    public bool Activo { get; set; }

    public virtual PagoRecibido IdPagoNavigation { get; set; } = null!;

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
}
