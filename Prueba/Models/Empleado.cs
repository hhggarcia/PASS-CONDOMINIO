using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Empleado
    {
        public int IdEmpleado { get; set; }
        public int IdCondominio { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public int Cedula { get; set; }
        public DateTime FechaIngreso { get; set; }
        public bool Estado { get; set; }
    }
}
