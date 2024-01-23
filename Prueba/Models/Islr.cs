using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Islr
{
    public int Id { get; set; }

    public decimal UnidadTributaria { get; set; }

    public decimal Factor { get; set; }

    public string Concepto { get; set; } = null!;

    public string Literal { get; set; } = null!;

    public string TipoReceptor { get; set; } = null!;

    public bool Residente { get; set; }

    public bool Domiciliada { get; set; }

    public decimal BaseImponible { get; set; }

    public decimal Tarifa { get; set; }

    public string? MontoSujeto { get; set; }

    public string? PagosMayores { get; set; }

    public string? Sustraendo { get; set; }

    public virtual ICollection<Producto> Productos { get; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; } = new List<Proveedor>();
}
