using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Propietario
    {
        public int IdPropietario { get; set; }
        public int IdUsuario { get; set; }
        public bool Solvencia { get; set; }
        public decimal Saldo { get; set; }
        public decimal Deuda { get; set; }
    }
}
