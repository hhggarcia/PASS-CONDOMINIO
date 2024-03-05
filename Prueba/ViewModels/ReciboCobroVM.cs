using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ReciboCobroVM
    {
        public Condominio condominio { get; set; }
        public Propiedad propiedad { get; set; }
        public IList<PropiedadesGrupo>? grupos { get; set; } = new List<PropiedadesGrupo>();
        public IList<Transaccion>? transaccions { get; set; } = new List<Transaccion>();
    }
}
