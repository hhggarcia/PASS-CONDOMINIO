using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class ReferenciasPr
    {
        public int IdReferencia { get; set; }
        public int IdPagoRecibido { get; set; }
        public int NumReferencia { get; set; }
        public string Banco { get; set; } = null!;

        public virtual PagoRecibido IdPagoRecibidoNavigation { get; set; } = null!;
    }
}
