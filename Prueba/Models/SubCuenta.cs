using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class SubCuenta
{
    [Required]
    public int Id { get; set; }

    [Required]
    public short IdCuenta { get; set; }

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    public string Descricion { get; set; } = string.Empty;

    [StringLength(maximumLength: 2, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    public string Codigo { get; set; } = string.Empty;

    public virtual ICollection<CodigoCuentasGlobal> CodigoCuentasGlobals { get; } = new List<CodigoCuentasGlobal>();

    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
}
