using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class NotaDebito
{
    public int IdNotaDebito { get; set; }

    public string NumNotaDebito { get; set; } = null!;

    public string Concepto { get; set; } = null!;

    public int IdProveedor { get; set; }

    public decimal Abonado { get; set; }

    public virtual ICollection<CompRetIvaCliente> CompRetIvaClientes { get; set; } = new List<CompRetIvaCliente>();

    public virtual ICollection<CompRetIva> CompRetIvas { get; set; } = new List<CompRetIva>();

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual ICollection<PagosNotaDebito> PagosNotaDebitos { get; set; } = new List<PagosNotaDebito>();
}
