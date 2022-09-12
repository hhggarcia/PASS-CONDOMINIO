using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class EstadoSituacion
    {
        public int IdEstSituacion { get; set; }
        public int IdActivo { get; set; }
        public int IdPasivo { get; set; }
        public int IdPatrimonio { get; set; }
        public decimal TotalAct { get; set; }
        public decimal TotalPas { get; set; }
        public decimal TotalPat { get; set; }
        public DateTime Fecha { get; set; }
    }
}
