using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Condominio
{
    public int IdCondominio { get; set; }

    public string IdAdministrador { get; set; } = null!;

    public string Rif { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal InteresMora { get; set; }

    public string Direccion { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool? ContribuyenteEspecial { get; set; }

    public virtual ICollection<BalanceComprobacion> BalanceComprobacions { get; set; } = new List<BalanceComprobacion>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; } = new List<CodigoCuentasGlobal>();

    public virtual ICollection<CuentasCobrar> CuentasCobrars { get; set; } = new List<CuentasCobrar>();

    public virtual ICollection<CuentasPagar> CuentasPagars { get; set; } = new List<CuentasPagar>();

    public virtual ICollection<CuotasEspeciale> CuotasEspeciales { get; set; } = new List<CuotasEspeciale>();

    public virtual ICollection<EstadoResultado> EstadoResultados { get; set; } = new List<EstadoResultado>();

    public virtual ICollection<EstadoSituacion> EstadoSituacions { get; set; } = new List<EstadoSituacion>();

    public virtual AspNetUser IdAdministradorNavigation { get; set; } = null!;

    public virtual ICollection<LibroCompra> LibroCompras { get; set; } = new List<LibroCompra>();

    public virtual ICollection<LibroVenta> LibroVenta { get; set; } = new List<LibroVenta>();

    public virtual ICollection<MonedaCond> MonedaConds { get; set; } = new List<MonedaCond>();

    public virtual ICollection<PagoEmitido> PagoEmitidos { get; set; } = new List<PagoEmitido>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Propiedad> Propiedads { get; set; } = new List<Propiedad>();

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();

    public virtual ICollection<RelacionGasto> RelacionGastos { get; set; } = new List<RelacionGasto>();
}
