using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class ReferenciasPe
    {
        public int IdReferencia { get; set; }
        public int IdPagoEmitido { get; set; }
        public int NumReferencia { get; set; }
        public string Banco { get; set; } = null!;

        public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
    }
}
