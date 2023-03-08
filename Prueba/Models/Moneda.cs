using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Moneda
{
    [Required]
    public int IdMoneda { get; set; }

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(maximumLength: 50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
    public string Pais { get; set; } = string.Empty;

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();
}
