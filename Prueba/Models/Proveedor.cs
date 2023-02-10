using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string Nombre { get; set; } = null!;

    public string Rif { get; set; } = null!;

    public int Telefono { get; set; }

    public string Descripcion { get; set; } = null!;
}
