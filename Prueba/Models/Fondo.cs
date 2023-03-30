using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Fondo
{
    public int IdFondo { get; set; }

    [Required]
    [Display(Name = "# Cuenta")]
    public int IdCodCuenta { get; set; }

    [Required]
    [Range(0, maximum: 100, ErrorMessage = "El porcentaje no puede ser superior a {2}%")]
    public int Porcentaje { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Desde")]
    [Required]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Hasta")]
    [Required]
    public DateTime FechaFin { get; set; }

    [Range(0, maximum: 100000000, ErrorMessage = "El Saldo no puede ser superior a {2}")]
    [Required]
    public decimal Saldo { get; set; } = 0;

    [Display(Name = "Cuenta")]
    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
}
