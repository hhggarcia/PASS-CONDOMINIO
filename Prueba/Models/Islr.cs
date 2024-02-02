using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Islr
{
    public int Id { get; set; }

    public decimal UnidadTributaria { get; set; }

    public decimal Factor { get; set; }

    public string Concepto { get; set; } = null!;

    public bool Pnatural { get; set; }

    public bool Pjuridica { get; set; }

    public bool Residenciada { get; set; }

    public bool NoResidenciada { get; set; }

    public bool Domiciliada { get; set; }

    public bool NoDomiciliada { get; set; }

    public decimal BaseImponible { get; set; }

    public string Modo { get; set; } = null!;

    public decimal MontoDesde { get; set; }

    public decimal MontoHasta { get; set; }

    public decimal Tarifa { get; set; }

    public decimal Sustraendo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
