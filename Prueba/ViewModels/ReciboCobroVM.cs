using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ReciboCobroVM
    {
        public Condominio condominio { get; set; }
        public Propiedad propiedad { get; set; }
        public RelacionGasto RelacionGasto { get; set; }
        public AspNetUser AspNetUser { get; set; }
        public RelacionGastoTransaccion RelacionGastoTransaccion { get; set; }
        public List<PropiedadesGrupo> grupos { get; set; } = new List<PropiedadesGrupo>();
        public List<Transaccion>? transaccions { get; set; } = new List<Transaccion>();
    }
}
