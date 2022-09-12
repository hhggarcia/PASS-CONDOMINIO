using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class PersonaJuntum
    {
        public int IdPJunta { get; set; }
        public int IdJunta { get; set; }
        public int IdPropietario { get; set; }
        public string Cargo { get; set; } = null!;
    }
}
