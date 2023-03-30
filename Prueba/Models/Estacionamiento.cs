using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Estacionamiento
{
    public int IdEstacionamiento { get; set; }

    [Required]
    [Display(Name = "# Inmueble")]
    public int IdInmueble { get; set; }

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "El nombre no puede pasar de {1} caracteres")]
    public string Nombre { get; set; } = null!;

    [Required]
    [Range(1, maximum: 100000000, ErrorMessage = "La cantidad de puestos no puede ser superior a {2}")]
    [Display(Name = "# Puestos")]
    public int NumPuestos { get; set; }

    [Display(Name = "Inmueble")]
    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual ICollection<PuestoE> PuestoEs { get; } = new List<PuestoE>();
}
