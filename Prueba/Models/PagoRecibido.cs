using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoRecibido
{
    public int IdPagoRecibido { get; set; }

    public int IdCondominio { get; set; }

    public bool FormaPago { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int IdSubCuenta { get; set; }

    public string Concepto { get; set; } = null!;

    public bool Confirmado { get; set; }

    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public byte[]? Imagen { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<PagoCobroTransito> PagoCobroTransitos { get; set; } = new List<PagoCobroTransito>();

    public virtual ICollection<PagoFacturaEmitida> PagoFacturaEmitida { get; set; } = new List<PagoFacturaEmitida>();

    public virtual PagoReserva? PagoReserva { get; set; }

    public virtual ICollection<PagosCuota> PagosCuota { get; set; } = new List<PagosCuota>();

    public virtual ICollection<PagosRecibo> PagosRecibos { get; set; } = new List<PagosRecibo>();

    public virtual ICollection<ReferenciasPr> ReferenciasPrs { get; set; } = new List<ReferenciasPr>();
}
