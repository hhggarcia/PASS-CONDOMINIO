using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public int IdCondominio { get; set; }

    public string Nombre { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Rif { get; set; } = null!;

    public int IdRetencionIslr { get; set; }

    public int IdRetencionIva { get; set; }

    public decimal Saldo { get; set; }

    public string Representante { get; set; } = null!;

    public virtual ICollection<Anticipo> Anticipos { get; } = new List<Anticipo>();

    public virtual ICollection<Factura> Facturas { get; } = new List<Factura>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Islr IdRetencionIslrNavigation { get; set; } = null!;

    public virtual Iva IdRetencionIvaNavigation { get; set; } = null!;
}
