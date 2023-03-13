using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PagoRecibido
{
    [Required]
    public int IdPagoRecibido { get; set; }

    [Required]
    public int IdPropiedad { get; set; }

    [Required]
    public bool FormaPago { get; set; }

    [Required]
    [Range(1, maximum: 1000000, ErrorMessage = "El monto no puede ser superior a {2}")]

    public decimal Monto { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    //[Range(typeof(DateTime), "1/1/2023", "12/31/2023", ErrorMessage = "la fecha debe estar entre {1} and {2}")]
    public DateTime Fecha { get; set; }

    [Required]
    public int IdSubCuenta { get; set; }

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    public string Concepto { get; set; } = string.Empty;

    public bool Confirmado { get; set; }

    public decimal? ValorDolar { get; set; }

    public decimal MontoRef { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual PagoReserva? PagoReserva { get; set; }

    public virtual ICollection<PagosRecibo> PagosRecibos { get; } = new List<PagosRecibo>();

    public virtual ICollection<ReferenciasPr> ReferenciasPrs { get; } = new List<ReferenciasPr>();
}
