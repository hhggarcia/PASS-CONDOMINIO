namespace Prueba.Models
{
    public class IndexPagosVM
    {
        public IList<PagoEmitido>? PagosEmitidos { get; set; }
        public IList<ReferenciasPe>? Referencias { get; set; }
        public IList<ReferenciaDolar>? ReferenciasDolar { get; set; }
        public IList<SubCuenta>? BancosCondominio { get; set; }
    }
}