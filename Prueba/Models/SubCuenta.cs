using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class SubCuenta
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "# Cuenta")]
    public short IdCuenta { get; set; }

    [Required]
    [Display(Name = "Descripción")]
    [StringLength(maximumLength: 100, ErrorMessage = "La Descripción no puede pasar de {1} caracteres")]
    public string Descricion { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Código")]
    [StringLength(maximumLength: 2, ErrorMessage = "El código no puede pasar de {1} caracteres")]
    public string Codigo { get; set; } = string.Empty;

    public decimal? Saldo { get; set; } = 0;

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();

    [Display(Name = "Cuenta")]
    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
}
