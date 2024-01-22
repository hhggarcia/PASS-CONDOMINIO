using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Islr
{
    public int Id { get; set; }

    public decimal UnidadTributaria { get; set; }

    public decimal Factor { get; set; }

    public decimal MontoMin { get; set; }

    public bool Activo { get; set; }

    public string Concepto { get; set; } = null!;

    public string Literal { get; set; } = null!;

    public int Porcentaje { get; set; }

    public bool Pnatural { get; set; }

    public bool Pjuridica { get; set; }

    public virtual ICollection<Producto> Productos { get; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; } = new List<Proveedor>();
}
