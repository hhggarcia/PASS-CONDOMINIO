using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Provision
{
    [Display(Name = "# Provisión")]
    [Required]
    public int IdProvision { get; set; }
    
    [Display(Name = "Gasto")]
    [Required]
    public int IdCodGasto { get; set; }
    
    [Display(Name = "Provisión")]
    [Required]
    public int IdCodCuenta { get; set; }

    [Range(1, maximum: 1000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Required]
    public decimal Monto { get; set; }
    
    [DataType(DataType.DateTime)]
    //[Range(typeof(DateTime), "1/1/2023", "12/31/2023", ErrorMessage = "la fecha debe estar entre {1} and {2}")]

    [Display(Name = "Desde")]
    [Required]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.DateTime)]
    //[Range(typeof(DateTime), "1/1/2023", "12/31/2023", ErrorMessage = "la fecha debe estar entre {1} and {2}")]

    [Display(Name = "Hasta")]
    [Required]
    public DateTime FechaFin { get; set; }

    public decimal MontoRef { get; set; }

    public decimal ValorDolar { get; set; }
    
    [Display(Name = "Tipo moneda")]
    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    public virtual CodigoCuentasGlobal IdCodGastoNavigation { get; set; } = null!;
}
