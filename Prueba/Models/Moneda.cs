using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class Moneda
{
    public int IdMoneda { get; set; }

    [StringLength(maximumLength: 50, ErrorMessage = "El nombre no puede pasar de {1} caracteres")]
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(maximumLength: 50, ErrorMessage = "El país no puede pasar de {1} caracteres")]
    [Required]
    public string Pais { get; set; } = string.Empty;

    public virtual ICollection<MonedaCond> MonedaConds { get; } = new List<MonedaCond>();

    public virtual ICollection<MonedaCuenta> MonedaCuenta { get; } = new List<MonedaCuenta>();
}
