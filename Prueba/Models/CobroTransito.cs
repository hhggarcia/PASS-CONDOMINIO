using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CobroTransito
{
    public int IdCobroTransito { get; set; }

    public int IdCondominio { get; set; }

    public bool FormaPago { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public string Concepto { get; set; } = null!;

    public bool Factura { get; set; }

    public bool Recibo { get; set; }

    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public bool Asignado { get; set; }

    public bool Activo { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<PagoCobroTransito> PagoCobroTransitos { get; set; } = new List<PagoCobroTransito>();
}
