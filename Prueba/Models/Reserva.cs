﻿using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdPropiedad { get; set; }

    public int IdArea { get; set; }

    public string TipoEvento { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual AreaComun IdAreaNavigation { get; set; } = null!;

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual ICollection<ReciboReserva> ReciboReservas { get; } = new List<ReciboReserva>();
}