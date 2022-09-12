using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class ReciboNomina
    {
        public int IdReciboNomina { get; set; }
        public int IdCondominio { get; set; }
        public DateTime Fecha { get; set; }
        public bool Entregado { get; set; }
    }
}
