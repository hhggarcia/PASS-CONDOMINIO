using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class MonedaCuenta
{
    public int Id { get; set; }

    /// <summary>
    /// Codigo Sub cuenta del condominio
    /// </summary>
    [Display(Name=" # Cuenta")]
    [Required]
    public int IdCodCuenta { get; set; }

    /// <summary>
    /// Moneda asignada
    /// </summary>
    [Display(Name="# Moneda")]
    [Required]
    public int IdMoneda { get; set; }

    /// <summary>
    /// Mostrar cuenta en pago del propietario
    /// </summary>
    public bool RecibePagos { get; set; }

    [Display(Name ="Cuenta")]
    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    [Display(Name ="Moneda")]
    public virtual Moneda IdMonedaNavigation { get; set; } = null!;
}
