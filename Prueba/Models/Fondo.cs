using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Fondo
{
    [Display(Name = "# Fondo")]
    public int IdFondo { get; set; }

    [Display(Name = "# Cuenta")]
    public int IdCodCuenta { get; set; }

    [Range(1, maximum: 100, ErrorMessage = "El monto no puede ser superior a {2} %")]
    public int Porcentaje { get; set; }

    [Display(Name = "Desde")]
    [DataType(DataType.DateTime)]
    [Range(typeof(DateTime), "1/1/2023", "12/31/2023",
        ErrorMessage = "la fecha debe estar entre {1} and {2}")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Hasta")]
    [DataType(DataType.DateTime)]
    [Range(typeof(DateTime), "1/1/2023", "12/31/2023", ErrorMessage = "la fecha debe estar entre {1} and {2}")]
    public DateTime FechaFin { get; set; }

    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;
}
