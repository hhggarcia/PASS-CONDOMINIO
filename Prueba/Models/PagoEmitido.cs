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

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<PagoAnticipo> PagoAnticipos { get; } = new List<PagoAnticipo>();

    public virtual ICollection<PagoFactura> PagoFacturas { get; } = new List<PagoFactura>();

    public virtual ICollection<PagosNomina> PagosNominas { get; } = new List<PagosNomina>();

    public virtual ICollection<ReferenciasPe> ReferenciasPes { get; } = new List<ReferenciasPe>();
}
