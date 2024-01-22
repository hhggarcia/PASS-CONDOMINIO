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

    public decimal? InteresMora { get; set; }

    public virtual ICollection<BalanceComprobacion> BalanceComprobacions { get; } = new List<BalanceComprobacion>();

    public virtual ICollection<CuentasCobrar> CuentasCobrars { get; } = new List<CuentasCobrar>();

    public virtual ICollection<CuentasCondominio> CuentasCondominios { get; } = new List<CuentasCondominio>();

    public virtual ICollection<CuentasPagar> CuentasPagars { get; } = new List<CuentasPagar>();

    public virtual ICollection<CuotasEspeciale> CuotasEspeciales { get; } = new List<CuotasEspeciale>();

    public virtual ICollection<EstadoResultado> EstadoResultados { get; } = new List<EstadoResultado>();

    public virtual ICollection<EstadoSituacion> EstadoSituacions { get; } = new List<EstadoSituacion>();

    public virtual AspNetUser IdAdministradorNavigation { get; set; } = null!;

    public virtual ICollection<Inmueble> Inmuebles { get; } = new List<Inmueble>();

    public virtual ICollection<LibroCompra> LibroCompras { get; } = new List<LibroCompra>();

    public virtual ICollection<LibroVenta> LibroVenta { get; } = new List<LibroVenta>();

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();

    public virtual ICollection<PagoEmitido> PagoEmitidos { get; } = new List<PagoEmitido>();

    public virtual ICollection<Producto> Productos { get; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; } = new List<Proveedor>();

    public virtual ICollection<RelacionGasto> RelacionGastos { get; } = new List<RelacionGasto>();
}
