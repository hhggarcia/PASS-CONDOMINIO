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

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
