using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public int IdCondominio { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Rif { get; set; } = string.Empty;

    public int? IdRetencionIslr { get; set; }

    public int? IdRetencionIva { get; set; }

    public decimal Saldo { get; set; }

    public string Representante { get; set; } = string.Empty;

    public bool ContribuyenteEspecial { get; set; }

    public bool Beneficiario { get; set; }

    public virtual ICollection<Anticipo> Anticipos { get; set; } = new List<Anticipo>();

    public virtual ICollection<CompRetIva> CompRetIvas { get; set; } = new List<CompRetIva>();

    public virtual ICollection<ComprobanteRetencion> ComprobanteRetencions { get; set; } = new List<ComprobanteRetencion>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Islr? IdRetencionIslrNavigation { get; set; }

    public virtual Iva? IdRetencionIvaNavigation { get; set; }

    public virtual ICollection<NotaDebito> NotaDebitos { get; set; } = new List<NotaDebito>();

    public virtual ICollection<OrdenPago> OrdenPagos { get; set; } = new List<OrdenPago>();

    public virtual ICollection<Transaccion> Transaccions { get; set; } = new List<Transaccion>();
}
