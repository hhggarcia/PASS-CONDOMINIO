using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class MonedaCond
{
    [Display(Name = "# Moneda")]
    [Required]
    public int IdMonedaCond { get; set; }
    
    [Display(Name = "Condominio")]
    [Required]
    public int IdCondominio { get; set; }
    
    [Display(Name = "Moneda")]
    [Required]
    public int IdMoneda { get; set; }
    
    [Display(Name = "Principal")]
    [Required]
    public bool Princinpal { get; set; }
    
    [Display(Name = "Símbolo")]
    [StringLength(maximumLength: 2, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    [Required]
    public string Simbolo { get; set; } = null!;

    /// <summary>
    /// Valor de la moneda respecto al dolar
    /// </summary>
    [Display(Name = "Tasa de Cambio $")]
    [Range(1, maximum: 1000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    [Required]
    public decimal ValorDolar { get; set; }

    [Display(Name = "Condominio")]

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
    [Display(Name = "Moneda")]

    public virtual Moneda IdMonedaNavigation { get; set; } = null!;
}
