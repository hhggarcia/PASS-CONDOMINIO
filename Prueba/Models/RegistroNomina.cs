using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class RegistroNomina
    {
        public int IdRegistroNomina { get; set; }
        public int IdReciboNomina { get; set; }
        public int IdEmpleado { get; set; }
        public TimeSpan Horas { get; set; }
        public short Dias { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Asignaciones { get; set; }
        public decimal Deducciones { get; set; }
    }
}
