using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PagoEmitido
{
    [Display(Name = "# Pago")]

    public int IdPagoEmitido { get; set; }

    [Display(Name = "# Condominio")]
    public int IdCondominio { get; set; }

    [Display(Name = "# Proveedor")]
    public int? IdProveedor { get; set; }

    [DataType(DataType.DateTime)]
    [Range(typeof(DateTime), "1/1/2023", "12/31/2023",
        ErrorMessage = "la fecha debe estar entre {1} and {2}")]
    public DateTime Fecha { get; set; }

    [Range(1, maximum: 1000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    public decimal Monto { get; set; }

    [Display(Name = "Forma de Pago")]
    public bool FormaPago { get; set; }

    [Display(Name = "Tasa de Cambio $")]
    public decimal ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReferenciasPe> ReferenciasPes { get; } = new List<ReferenciasPe>();
}
