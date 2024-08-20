using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoEmitido
{
    public int IdPagoEmitido { get; set; }

    public int IdCondominio { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Monto { get; set; }

    public bool FormaPago { get; set; }

    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public bool Activo { get; set; }

    public string? Concepto { get; set; }

    public virtual ICollection<AnticipoNomina> AnticipoNominas { get; set; } = new List<AnticipoNomina>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<OrdenPago> OrdenPagos { get; set; } = new List<OrdenPago>();

    public virtual ICollection<PagoAnticipo> PagoAnticipos { get; set; } = new List<PagoAnticipo>();

    public virtual ICollection<PagoFactura> PagoFacturas { get; set; } = new List<PagoFactura>();

    public virtual ICollection<PagosNomina> PagosNominas { get; set; } = new List<PagosNomina>();

    public virtual ICollection<PagosNotaDebito> PagosNotaDebitos { get; set; } = new List<PagosNotaDebito>();

    public virtual ICollection<ReferenciasPe> ReferenciasPes { get; set; } = new List<ReferenciasPe>();
}
