using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class PagoEmitido
{
    public int IdPagoEmitido { get; set; }

    public int IdCondominio { get; set; }

    public int? IdProveedor { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Monto { get; set; }

    public bool FormaPago { get; set; }

    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReferenciasPe> ReferenciasPes { get; } = new List<ReferenciasPe>();
}
