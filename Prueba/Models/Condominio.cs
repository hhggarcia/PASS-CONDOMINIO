using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Condominio
{
    public int IdCondominio { get; set; }

    [Required]
    [Display(Name ="# Administrador")]
    public string IdAdministrador { get; set; } = string.Empty;

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "El RIF no puede pasar de {1} caracteres")]
    public string Rif { get; set; } = string.Empty;

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "El tipo no puede pasar de {1} caracteres")]
    [Display(Name ="Tipo de Condominio")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "El nombre no puede pasar de {1} caracteres")] 
    public string Nombre { get; set; } = string.Empty;

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();

    public virtual ICollection<EstadoResultado> EstadoResultados { get; } = new List<EstadoResultado>();

    [Display(Name ="Administrador")]
    public virtual AspNetUser IdAdministradorNavigation { get; set; } = null!;

    public virtual ICollection<Inmueble> Inmuebles { get; } = new List<Inmueble>();

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();

    public virtual ICollection<PagoEmitido> PagoEmitidos { get; } = new List<PagoEmitido>();

    public virtual ICollection<RelacionGasto> RelacionGastos { get; } = new List<RelacionGasto>();
}
