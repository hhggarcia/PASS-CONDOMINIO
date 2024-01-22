using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Propiedad
{
    public int IdPropiedad { get; set; }

    public int IdInmueble { get; set; }

    public string IdUsuario { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public decimal Dimensiones { get; set; }

    public decimal Alicuota { get; set; }

    public bool Solvencia { get; set; }

    public decimal Saldo { get; set; }

    public decimal Deuda { get; set; }

    public decimal? MontoIntereses { get; set; }

    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<PagoRecibido> PagoRecibidos { get; } = new List<PagoRecibido>();

    public virtual ICollection<PuestoE> PuestoEs { get; } = new List<PuestoE>();

    public virtual ICollection<ReciboCobro> ReciboCobros { get; } = new List<ReciboCobro>();

    public virtual ICollection<ReciboCuota> ReciboCuota { get; } = new List<ReciboCuota>();

    public virtual ICollection<Reserva> Reservas { get; } = new List<Reserva>();
}
