using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Transaccion
{
    public int IdTransaccion { get; set; }

    public bool TipoTransaccion { get; set; }

    public int? IdPropiedad { get; set; }

    public int IdCodCuenta { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? IdProveedor { get; set; }

    public string Documento { get; set; } = null!;

    public decimal MontoTotal { get; set; }

    public decimal Cancelado { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual Propiedad? IdPropiedadNavigation { get; set; }

    public virtual Proveedor? IdProveedorNavigation { get; set; }

    public virtual ICollection<RelacionGastoTransaccion> RelacionGastoTransaccions { get; set; } = new List<RelacionGastoTransaccion>();
}
