using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Propiedad
{
    public int IdPropiedad { get; set; }

    [Display(Name ="# Inmueble")]
    [Required]
    public int IdInmueble { get; set; }

    [Display(Name ="# Usuario")]
    [Required]
    public string IdUsuario { get; set; } = string.Empty;

    [Display(Name ="Código")]
    [StringLength(maximumLength: 10, ErrorMessage = "El código no puede pasar de {1} caracteres")]
    [Required]
    public string Codigo { get; set; } = string.Empty;

    [Range(0, maximum: 100000000, ErrorMessage = "La dimensión no puede ser mayor de {2}")]
    [Required]
    public decimal Dimensiones { get; set; }

    [Display(Name ="Alícuota")]
    [Range(0, maximum: 100, ErrorMessage = "La alícuota no puede ser mayor de {2}%")]
    [Required]
    public decimal Alicuota { get; set; }

    public bool Solvencia { get; set; }

    [Range(0, maximum: 100000000, ErrorMessage = "El Saldo no puede ser mayor de {2}")]
    public decimal Saldo { get; set; }

    [Range(0, maximum: 100000000, ErrorMessage = "La Deuda no puede ser mayor de {2}")]
    public decimal Deuda { get; set; }

    [Display(Name ="Inmueble")]
    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    [Display(Name ="Usuario")]
    public virtual AspNetUser IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<PagoRecibido> PagoRecibidos { get; } = new List<PagoRecibido>();

    public virtual ICollection<PuestoE> PuestoEs { get; } = new List<PuestoE>();

    public virtual ICollection<ReciboCobro> ReciboCobros { get; } = new List<ReciboCobro>();

    public virtual ICollection<Reserva> Reservas { get; } = new List<Reserva>();
}
