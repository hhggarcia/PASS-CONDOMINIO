using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class ReciboCobro
{
    public int IdReciboCobro { get; set; }

    [Display(Name = "# Propiedad")]
    [Required]
    public int IdPropiedad { get; set; }

    [Display(Name = "# Relación de Gastos")]
    [Required]
    public int IdRgastos { get; set; }

    [Required]
    public decimal Monto { get; set; }

    [DataType(DataType.DateTime)]
    [Required]
    public DateTime Fecha { get; set; }

    public bool Pagado { get; set; }

    public bool EnProceso { get; set; }

    public decimal Abonado { get; set; }

    [Display(Name ="Monto Referencia")]
    [Required]
    public decimal MontoRef { get; set; }

    [Display(Name ="Tasa de Cambio")]
    [Required]
    public decimal ValorDolar { get; set; }

    [Required]
    public string SimboloMoneda { get; set; } = string.Empty;

    [Required]
    public string SimboloRef { get; set; } = string.Empty;

    [Display(Name ="Propiedad")]
    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    [Display(Name ="Relación de Gastos")]
    public virtual RelacionGasto IdRgastosNavigation { get; set; } = null!;

    public virtual ICollection<PagosRecibo> PagosRecibos { get; } = new List<PagosRecibo>();
}
