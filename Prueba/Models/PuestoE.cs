using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PuestoE
{
    public int IdPuestoE { get; set; }

    [Display(Name = "# Estacionamiento")]
    public int IdEstacionamiento { get; set; }

    [Display(Name = "# Propiedad")]
    public int IdPropiedad { get; set; }

    [Display(Name = "Código")]
    [StringLength(maximumLength: 10, ErrorMessage = "El código no puede pasar de {1} caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [Range(0, maximum: 100, ErrorMessage = "La alícuota no puede ser mayor de {2} %")]
    public decimal Alicuota { get; set; }

    [Display(Name = "Estacionamiento")]
    public virtual Estacionamiento IdEstacionamientoNavigation { get; set; } = null!;

    [Display(Name = "Propiedad")]
    public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
}
