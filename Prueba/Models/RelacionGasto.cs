using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class RelacionGasto
{
    public int IdRgastos { get; set; }

    [Required]
    [Display(Name = "Sub Total")]
    public decimal SubTotal { get; set; }

    [Required]
    [Display(Name = "Total Mensual")]
    public decimal TotalMensual { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Fecha { get; set; }

    [Required]
    [Display(Name = "# Condominio")]
    public int IdCondominio { get; set; }

    [Display(Name = "Monto Referencia")]
    public decimal MontoRef { get; set; }

    [Display(Name = "Tasa de Cambio")]
    public decimal ValorDolar { get; set; }

    public string SimboloMoneda { get; set; } = string.Empty;

    public string SimboloRef { get; set; } = string.Empty;

    [Display(Name = "Condominio")]
    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReciboCobro> ReciboCobros { get; } = new List<ReciboCobro>();
}
