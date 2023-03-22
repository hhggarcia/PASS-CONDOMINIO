using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class MonedaCond
{
    public int IdMonedaCond { get; set; }

    [Display(Name = "# Condominio")]
    [Required]
    public int IdCondominio { get; set; }

    [Display(Name = "# Moneda")]
    [Required]
    public int IdMoneda { get; set; }

    [Display(Name = "Principal")]
    [Required]
    public bool Princinpal { get; set; }

    [StringLength(maximumLength: 2, ErrorMessage = "El símbolo no puede pasar de {1} caracteres")]
    [Required]
    public string Simbolo { get; set; } = string.Empty;

    /// <summary>
    /// Valor de la moneda respecto al dolar
    /// </summary>
    [Range(1, maximum: 100000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Required]
    public decimal ValorDolar { get; set; }

    [Display(Name = "Condominio")]
    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    [Display(Name = "Moneda")]
    public virtual Moneda IdMonedaNavigation { get; set; } = null!;
}
