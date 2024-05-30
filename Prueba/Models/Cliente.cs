using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

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

    public string? Email { get; set; }

    public decimal? Deuda { get; set; }

    public bool? Solvente { get; set; }

    public virtual ICollection<CompRetIvaCliente> CompRetIvaClientes { get; set; } = new List<CompRetIvaCliente>();

    public virtual ICollection<ComprobanteRetencionCliente> ComprobanteRetencionClientes { get; set; } = new List<ComprobanteRetencionCliente>();

    public virtual ICollection<FacturaEmitida> FacturaEmitida { get; set; } = new List<FacturaEmitida>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Islr? IdRetencionIslrNavigation { get; set; }

    public virtual Iva? IdRetencionIvaNavigation { get; set; }

    public virtual ICollection<NotaCredito> NotaCreditos { get; set; } = new List<NotaCredito>();
}
