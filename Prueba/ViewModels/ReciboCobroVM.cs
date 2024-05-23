using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ReciboCobroVM
    {
        public Condominio? Condominio { get; set; }
        public Propiedad? Propiedad { get; set; }
        public RelacionGasto? RelacionGasto { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        public RelacionGastoTransaccion? RelacionGastoTransaccion { get; set; }
        public List<PropiedadesGrupo> Grupos { get; set; } = new List<PropiedadesGrupo>();
        public List<Transaccion>? Transaccions { get; set; } = new List<Transaccion>();
        public List<Provision> Provisiones { get; set; } = new List<Provision>();
    }
}
