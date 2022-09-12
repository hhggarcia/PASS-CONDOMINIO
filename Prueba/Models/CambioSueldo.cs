using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class CambioSueldo
    {
        public int IdCsueldo { get; set; }
        public int IdEmpleado { get; set; }
        public string Cargo { get; set; } = null!;
        public decimal Salario { get; set; }
        public DateTime Fecha { get; set; }
    }
}
