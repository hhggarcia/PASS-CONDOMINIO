using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Inmueble
{
    public int IdInmueble { get; set; }

    [Display(Name = "# Zona")]
    [Required]
    public int IdZona { get; set; }

    [StringLength(maximumLength: 50, ErrorMessage = "El nombre no puede pasar de {1} caracteres")]
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Total de Propiedades")]
    [Required]
    public int TotalPropiedad { get; set; }

    [Display(Name = "# Condominio")]
    [Required]
    public int IdCondominio { get; set; }

    public virtual ICollection<AreaComun> AreaComuns { get; } = new List<AreaComun>();

    public virtual ICollection<Estacionamiento> Estacionamientos { get; } = new List<Estacionamiento>();

    [Display(Name = "Condominio")]
    public virtual Condominio IdCondominioNavigation { get; set; } = null!;

    [Display(Name = "Inmueble")]
    public virtual Zona IdInmuebleNavigation { get; set; } = null!;

    public virtual ICollection<Propiedad> Propiedads { get; } = new List<Propiedad>();
}
