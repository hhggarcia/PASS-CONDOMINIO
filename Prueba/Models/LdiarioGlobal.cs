using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class LdiarioGlobal
{
    public int IdAsiento { get; set; }

    [Display(Name ="# Cuenta")]
    [Required]
    public int IdCodCuenta { get; set; }

    [DataType(DataType.DateTime)]
    [Required]
    public DateTime Fecha { get; set; }

    [StringLength(maximumLength: 50, ErrorMessage = "El país no puede pasar de {1} caracteres")]
    [Required]
    public string Concepto { get; set; } = string.Empty;

    [Range(1, maximum: 100000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Required]
    public decimal Monto { get; set; }

    [Display(Name ="Tipo de Operación")]
    [Required]
    public bool TipoOperacion { get; set; }

    [Range(1, maximum: int.MaxValue, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Display(Name ="# Asiento")]
    [Required]
    public int NumAsiento { get; set; }

    [Range(1, maximum: int.MaxValue, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Display(Name ="Monto Referencia")]
    [Required]
    public decimal MontoRef { get; set; }

    [Display(Name = "Tasa de Cambio")]
    [Required] 
    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = string.Empty;

    public string SimboloRef { get; set; } = string.Empty;

    public virtual ICollection<Activo> Activos { get; } = new List<Activo>();

    public virtual ICollection<Gasto> Gastos { get; } = new List<Gasto>();

    [Display(Name ="Cuenta")]
    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual ICollection<Ingreso> Ingresos { get; } = new List<Ingreso>();

    public virtual ICollection<Pasivo> Pasivos { get; } = new List<Pasivo>();

    public virtual ICollection<Patrimonio> Patrimonios { get; } = new List<Patrimonio>();
}
