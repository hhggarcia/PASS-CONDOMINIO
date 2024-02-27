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

    public int? IdRetencionIslr { get; set; }

    public int? IdRetencionIva { get; set; }

    public decimal Saldo { get; set; }

    public string Representante { get; set; } = null!;

    public bool ContribuyenteEspecial { get; set; }

    public virtual ICollection<Anticipo> Anticipos { get; set; } = new List<Anticipo>();

    public virtual ICollection<ComprobanteRetencion> ComprobanteRetencions { get; set; } = new List<ComprobanteRetencion>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Islr? IdRetencionIslrNavigation { get; set; }

    public virtual Iva? IdRetencionIvaNavigation { get; set; }
}
