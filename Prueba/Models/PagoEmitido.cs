using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class PagoEmitido
    {
        public int IdPagoEmitido { get; set; }
        public int IdCondominio { get; set; }
        public int? IdProveedor { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public bool FormaPago { get; set; }

        public virtual Condominio IdCondominioNavigation { get; set; } = new Condominio
        {
            IdCondominio = 0,
            IdAdministrador = "aux",
            Rif = "aux",
            Tipo = "aux",
            Nombre = "aux"

        };
        public virtual ReferenciasPe ReferenciasPe { get; set; } = new ReferenciasPe
        {
            IdPagoEmitido = 0,
            NumReferencia= 0,
            Banco = "aux"
        };
    }
}
