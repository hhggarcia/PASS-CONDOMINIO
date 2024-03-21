using Prueba.Models;

namespace Prueba.ViewModels
{
    public class IndexPagoFacturaEmitidaVM
    {
        public IList<PagoRecibido>? PagosRecibidos { get; set; }
        public IList<ReferenciasPr>? Referencias { get; set; }
        //public IList<ReferenciaDolar>? ReferenciasDolar { get; set; }
        public IList<SubCuenta>? BancosCondominio { get; set; }
    }
}
