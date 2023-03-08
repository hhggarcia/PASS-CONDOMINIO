using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Estacionamiento
{
    [Display(Name = "# Estacionamiento")]
    [Required]
    public int IdEstacionamiento { get; set; }

    [Display(Name = "# Inmueble")]
    [Required]
    public int IdInmueble { get; set; }

    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    [Required]
    public string Nombre { get; set; } = null!;

    [Required]
    [Display(Name = "Cant. Puestos")]
    [Range(1, maximum: 1000000, ErrorMessage = "El monto no puede ser superior a {2}")]
    public int NumPuestos { get; set; }

    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual ICollection<PuestoE> PuestoEs { get; } = new List<PuestoE>();
}
