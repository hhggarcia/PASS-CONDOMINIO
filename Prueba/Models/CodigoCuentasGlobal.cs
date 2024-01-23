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

    public virtual ICollection<CuentasGrupo> CuentasGrupos { get; } = new List<CuentasGrupo>();

    public virtual ICollection<Fondo> Fondos { get; } = new List<Fondo>();

    public virtual Clase IdClaseNavigation { get; set; } = null!;

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;

    public virtual Grupo IdGrupoNavigation { get; set; } = null!;

    public virtual SubCuenta IdSubCuentaNavigation { get; set; } = null!;

    public virtual ICollection<LdiarioGlobal> LdiarioGlobals { get; } = new List<LdiarioGlobal>();

    public virtual ICollection<MonedaCuenta> MonedaCuenta { get; } = new List<MonedaCuenta>();

    public virtual ICollection<Provision> ProvisioneIdCodCuentaNavigations { get; } = new List<Provision>();

    public virtual ICollection<Provision> ProvisioneIdCodGastoNavigations { get; } = new List<Provision>();
}
