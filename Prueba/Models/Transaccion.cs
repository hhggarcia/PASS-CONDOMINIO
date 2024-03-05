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

    public string MontoTotal { get; set; } = null!;

    public decimal Cancelado { get; set; }

    public bool FormaPago { get; set; }

    public int IdCodCuentaBanco { get; set; }

    public string NumDocumento { get; set; } = null!;

    public int IdGrupoGasto { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaBancoNavigation { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual GrupoGasto IdGrupoGastoNavigation { get; set; } = null!;

    public virtual Propiedad? IdPropiedadNavigation { get; set; }

    public virtual Proveedor? IdProveedorNavigation { get; set; }

    public virtual ICollection<RelacionGastoTransaccion> RelacionGastoTransaccions { get; set; } = new List<RelacionGastoTransaccion>();
}
