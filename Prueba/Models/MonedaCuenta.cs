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
    [Display(Name = "# Moneda")]
    [Required]
    public int IdMoneda { get; set; }

    /// <summary>
    /// Mostrar cuenta en pago del propietario
    /// </summary>
    [Display(Name = "Recibe pagos")]

    public bool RecibePagos { get; set; }

    [Display(Name = "Saldo Inicial")]
    [Range(0, maximum: 100000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Required]

    public decimal SaldoInicial { get; set; }

    [Display(Name = "Saldo")]
    [Range(0, maximum: 100000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Required]
    public decimal SaldoFinal { get; set; }

    [Display(Name = "Cuenta")]
    public virtual CodigoCuentasGlobal IdCodCuentaNavigation { get; set; } = null!;

    [Display(Name = "Moneda")]
    public virtual MonedaCond IdMonedaNavigation { get; set; } = null!;
}
