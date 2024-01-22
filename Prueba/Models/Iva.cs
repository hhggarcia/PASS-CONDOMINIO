using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Iva
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal Porcentaje { get; set; }

    public bool Activo { get; set; }

    public bool Principal { get; set; }

    public virtual ICollection<Producto> Productos { get; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; } = new List<Proveedor>();
}
