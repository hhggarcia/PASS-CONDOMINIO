using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Gasto
    {
        public int IdGasto { get; set; }
        public int IdAsiento { get; set; }

        public virtual LdiarioGlobal IdAsientoNavigation { get; set; } = null!;
    }
}
