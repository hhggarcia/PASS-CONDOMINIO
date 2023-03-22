using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Provision
{
    public int IdProvision { get; set; }

    [Display(Name ="# Cuenta Gasto")]
    [Required]
    public int IdCodGasto { get; set; }

    [Display(Name ="# Cuenta Provisión")]
    [Required]
    public int IdCodCuenta { get; set; }

    [Range(0, maximum: 100000000, ErrorMessage = "El monto no puede ser mayor de {2}")]
    [Required]
    public decimal Monto { get; set; }

    [Display(Name ="Desde")]
    [Required]
    public DateTime FechaInicio { get; set; }

    [Display(Name ="Hasta")]
    [Required]
    public DateTime FechaFin { get; set; }

    [Display(Name ="Monto Referencia")]
    public decimal MontoRef { get; set; }

    [Display(Name = "Tasa Provisión")]
    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    [Display(Name ="Cuenta Provisión")]
    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    [Display(Name ="Cuenta Gasto")]
    public virtual CodigoCuentasGlobal IdCodGastoNavigation { get; set; } = null!;
}
