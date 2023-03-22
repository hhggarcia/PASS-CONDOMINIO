using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class ReferenciasPr
{
    public int IdReferencia { get; set; }
    
    [Required]
    [Display(Name ="# Pago")]
    public int IdPagoRecibido { get; set; }

    [Required]
    [Display(Name ="# Referencia")]
    [Range(1, maximum: 999999, ErrorMessage = "El número de referencia debe ser de 6 dígitos")]
    public int NumReferencia { get; set; }

    [Required]
    public string Banco { get; set; } = string.Empty;

    [Display(Name ="Pago")]
    public virtual PagoRecibido IdPagoRecibidoNavigation { get; set; } = null!;
}
