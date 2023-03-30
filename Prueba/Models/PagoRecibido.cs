using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PagoRecibido
{
    public int IdPagoRecibido { get; set; }

    [Required]
    [Display(Name = "# Propiedad")]
    public int IdPropiedad { get; set; }

    [Required]
    [Display(Name = "Forma de Pago")]
    public bool FormaPago { get; set; }

    [Required]
    [Range(0, maximum: 100000000, ErrorMessage = "El monto no puede ser mayor de {2}")]
    public decimal Monto { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Fecha { get; set; }

    //[Required]
    [Display(Name = "Cuenta")]
    public int IdSubCuenta { get; set; }

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "El concepto no puede pasar de {1} caracteres")]
    public string Concepto { get; set; } = string.Empty;

    public bool Confirmado { get; set; }

    [Display(Name = "Tasa de Cambio")]
    public decimal ValorDolar { get; set; }

    [Display(Name = "Monto Referencia")]
    public decimal MontoRef { get; set; }

    public string SimboloMoneda { get; set; } = string.Empty;

    public string SimboloRef { get; set; } = string.Empty;

    [Display(Name = "Propiedad")]
    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;

    public virtual PagoReserva? PagoReserva { get; set; }

    public virtual ICollection<PagosRecibo> PagosRecibos { get; } = new List<PagosRecibo>();

    public virtual ICollection<ReferenciasPr> ReferenciasPrs { get; } = new List<ReferenciasPr>();
}
