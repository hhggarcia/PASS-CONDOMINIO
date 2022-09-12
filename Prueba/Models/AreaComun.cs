using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class AreaComun
    {
        public int IdAcomun { get; set; }
        public int IdInmueble { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Disponible { get; set; }
    }
}
