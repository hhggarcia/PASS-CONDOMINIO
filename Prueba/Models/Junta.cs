using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Junta
    {
        public int IdJunta { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }
}
