using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class EstadoResultado
    {
        public int IdEstResultado { get; set; }
        public int IdIngreso { get; set; }
        public int IdGasto { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public DateTime Fecha { get; set; }
    }
}
