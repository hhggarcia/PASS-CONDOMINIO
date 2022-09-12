using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Ingreso
    {
        public int IdIngreso { get; set; }
        public int IdAsiento { get; set; }

        public virtual LdiarioGlobal IdAsientoNavigation { get; set; } = null!;
    }
}
