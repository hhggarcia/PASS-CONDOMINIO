using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class CodigoCuentasGlobal
{
    public int IdCodCuenta { get; set; }

    public int IdSubCuenta { get; set; }

    public short IdCuenta { get; set; }

    public short IdGrupo { get; set; }

    public short IdClase { get; set; }

    public string Codigo { get; set; } = null!;

    public decimal Saldo { get; set; }

    public decimal SaldoInicial { get; set; }

    public int IdCondominio { get; set; }

    public virtual ICollection<Anticipo> Anticipos { get; set; } = new List<Anticipo>();

    public virtual ICollection<CuentasGrupo> CuentasGrupos { get; set; } = new List<CuentasGrupo>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual ICollection<Fondo> Fondos { get; set; } = new List<Fondo>();

    public virtual Clase IdClaseNavigation { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;

    public virtual Grupo IdGrupoNavigation { get; set; } = null!;

    public virtual SubCuenta IdSubCuentaNavigation { get; set; } = null!;

    public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; set; } = new List<LdiarioGlobal>();

    public virtual ICollection<MonedaCuenta> MonedaCuenta { get; set; } = new List<MonedaCuenta>();

    public virtual ICollection<Provision> ProvisioneIdCodCuentaNavigations { get; set; } = new List<Provision>();

    public virtual ICollection<Provision> ProvisioneIdCodGastoNavigations { get; set; } = new List<Provision>();
}
