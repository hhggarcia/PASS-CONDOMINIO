using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdCondominio { get; set; }

    public decimal Precio { get; set; }

    public int TipoProducto { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool Disponible { get; set; }

    public int IdRetencionIva { get; set; }

    public int IdRetencionIslr { get; set; }

    public virtual ICollection<FacturaEmitida> FacturaEmitida { get; set; } = new List<FacturaEmitida>();

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Islr IdRetencionIslrNavigation { get; set; } = null!;

    public virtual Iva IdRetencionIvaNavigation { get; set; } = null!;
}
