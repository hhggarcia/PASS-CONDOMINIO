using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class RelacionGasto
{
    [Display(Name = "# Relación gastos")]
    public int IdRgastos { get; set; }

    [Display(Name = "Sub Total")]
    public decimal SubTotal { get; set; }

    [Display(Name = "Total Mensual")]
    public decimal TotalMensual { get; set; }

    public DateTime Fecha { get; set; }

    [Display(Name = "# Condominio")]

    public int IdCondominio { get; set; }

    [Display(Name = "Monto Referencia")]
    public decimal MontoRef { get; set; }

    [Display(Name = "Tasa de cambio $")]
    public decimal? ValorDolar { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    public virtual ICollection<ReciboCobro> ReciboCobros { get; } = new List<ReciboCobro>();
}
