using Prueba.Validates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PagoEmitido
{
    public int IdPagoEmitido { get; set; }

    [Display(Name = "# Condominio")]
    public int IdCondominio { get; set; }

    [Display(Name = "# Proveedor")]
    public int? IdProveedor { get; set; }

    [Required]
    [FechaPagoEmitido]
    public DateTime Fecha { get; set; }

    [Required]
    [Range(1, maximum: 100000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    public decimal Monto { get; set; }

    [Required]
    [Display(Name = "Forma de Pago")]
    public bool FormaPago { get; set; }

    [Display(Name = "Tasa de Cambio")]
    public decimal ValorDolar { get; set; }

    [Display(Name = "Monto Referencia")]
    public decimal MontoRef { get; set; }

    public string SimboloMoneda { get; set; } = null!;

    public string SimboloRef { get; set; } = null!;

    [Display(Name = "Condominio")]
    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReferenciasPe> ReferenciasPes { get; } = new List<ReferenciasPe>();
}
