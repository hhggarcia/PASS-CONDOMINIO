using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class ReferenciasPe
{
    public int IdReferencia { get; set; }

    [Display(Name = "# Pago")]
    public int IdPagoEmitido { get; set; }

    [Display(Name = "# Referencia")]
    [Required]
    [Range(1, maximum: 999999, ErrorMessage = "El número de referencia debe ser de 6 dígitos")]
    public int NumReferencia { get; set; }

    [Required]
    public string Banco { get; set; } = string.Empty;

    [Display(Name = "Pago")]
    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
}
