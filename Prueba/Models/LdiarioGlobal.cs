using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class LdiarioGlobal
{
    [Display(Name = "Asiento")]
    public int IdAsiento { get; set; }

    [Display(Name = "# Cuenta")]
    public int IdCodCuenta { get; set; }

    [DataType(DataType.DateTime)]
    [Range(typeof(DateTime), "1/1/2023", "12/31/2023", ErrorMessage = "la fecha debe estar entre {1} and {2}")]
    public DateTime Fecha { get; set; }

    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    public string Concepto { get; set; } = null!;

    [Range(1, maximum: 1000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    public decimal Monto { get; set; }

    [Display(Name = "Tipo de Operación")]
    public bool TipoOperacion { get; set; }
    
    [Display(Name = "# Asiento")]
    public int NumAsiento { get; set; }

    [Display(Name = "Monto Referencia")]
    public decimal MontoRef { get; set; }

    [Display(Name = "Tasa de Cambio $")]
    public decimal ValorDolar { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual ICollection<Activo> Activos { get; } = new List<Activo>();

    public virtual ICollection<Gasto> Gastos { get; } = new List<Gasto>();

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual ICollection<Ingreso> Ingresos { get; } = new List<Ingreso>();

    public virtual ICollection<Pasivo> Pasivos { get; } = new List<Pasivo>();

    public virtual ICollection<Patrimonio> Patrimonios { get; } = new List<Patrimonio>();
}
