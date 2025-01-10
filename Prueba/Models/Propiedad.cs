using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Propiedad
{
    public int IdPropiedad { get; set; }

    public int IdCondominio { get; set; }

    public string IdUsuario { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public decimal Dimensiones { get; set; }

    public decimal Alicuota { get; set; }

    public bool Solvencia { get; set; }

    public decimal Saldo { get; set; }

    public decimal Deuda { get; set; }

    public decimal MontoIntereses { get; set; }

    public decimal? MontoMulta { get; set; }

    public decimal? Creditos { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Inquilino> Inquilinos { get; set; } = new List<Inquilino>();

    public virtual ICollection<NotaCredito> NotaCreditos { get; set; } = new List<NotaCredito>();

    public virtual ICollection<PagoPropiedad> PagoPropiedads { get; set; } = new List<PagoPropiedad>();

    public virtual ICollection<PropiedadesGrupo> PropiedadesGrupos { get; set; } = new List<PropiedadesGrupo>();

    public virtual ICollection<ReciboCobro> ReciboCobros { get; set; } = new List<ReciboCobro>();

    public virtual ICollection<ReciboCuota> ReciboCuota { get; set; } = new List<ReciboCuota>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Transaccion> Transaccions { get; set; } = new List<Transaccion>();
}
