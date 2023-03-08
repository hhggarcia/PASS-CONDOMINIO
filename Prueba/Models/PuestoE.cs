using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PuestoE
{
    [Display(Name = "# Puesto")]
    [Required]
    public int IdPuestoE { get; set; }

    [Display(Name = "# Estacionamiento")]
    [Required]
    public int IdEstacionamiento { get; set; }

    [Display(Name = "# Propiedad")]
    [Required]
    public int IdPropiedad { get; set; }

    [Display(Name = "Código")]
    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    [Required]
    public string Codigo { get; set; } = string.Empty;

    [Display(Name = "Alícuota")]
    [Required]
    public decimal Alicuota { get; set; }

    public virtual Estacionamiento IdEstacionamientoNavigation { get; set; } = null!;

    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
}
