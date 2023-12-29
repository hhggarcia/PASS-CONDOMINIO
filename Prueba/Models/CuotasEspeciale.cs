using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class CuotasEspeciale
{
    public int IdCuotaEspecial { get; set; }

    public int? IdCondominio { get; set; }
    [Required]
    [Display(Name = "Descripción")]
    [StringLength(maximumLength: 100, ErrorMessage = "La Descripción no puede pasar de {1} caracteres")]
    public string? Descripcion { get; set; }
    [Required(ErrorMessage = "El campo CantidadCuotas es obligatorio.")]
    [Range(1, 32, ErrorMessage = "La cantidad de cuotas debe estar entre 1 y 32.")]
    public int? CantidadCuotas { get; set; }

    public decimal? SubCuotas { get; set; }
    public decimal? MontoMensual { get; set; }
    [Required(ErrorMessage = "El campo del monto es obligatorio.")]
    public decimal? MontoTotal { get; set; }
    [Required(ErrorMessage = "El campo fecha de creación es obligatorio.")]
    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }
    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }
    [Required]
    public bool? Activa { get; set; }

    public decimal? ValorDolar { get; set; }

    public string? SimboloMoneda { get; set; }

    public string? SimboloRef { get; set; }

    public virtual ICollection<PagoReciboCuota> PagoReciboCuota { get; } = new List<PagoReciboCuota>();

    public virtual ICollection<ReciboCuota> ReciboCuota { get; } = new List<ReciboCuota>();
}
