using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaCredito
{
    public int IdNotaCredito { get; set; }

    public int IdFactura { get; set; }

    public int IdCliente { get; set; }

    public string Concepto { get; set; } = null!;

    public string Comprobante { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public decimal Monto { get; set; }

    public int? IdRetIva { get; set; }

    public int? IdRetIslr { get; set; }

    public virtual ICollection<CompRetIvaCliente> CompRetIvaClientes { get; set; } = new List<CompRetIvaCliente>();

    public virtual ICollection<CompRetIva> CompRetIvas { get; set; } = new List<CompRetIva>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual FacturaEmitida IdFacturaNavigation { get; set; } = null!;

    public virtual Islr? IdRetIslrNavigation { get; set; }

    public virtual Iva? IdRetIvaNavigation { get; set; }
}
