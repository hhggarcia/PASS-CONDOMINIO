using Prueba.Models;

namespace Prueba.ViewModels
{
    public class RecibosCreadosVM
    {
        public IList<Propiedad>? Propiedades { get; set; }
        public IList<ReciboCobro>? Recibos { get; set; }
        public RelacionDeGastosVM? RelacionGastos { get; set; }
        public TransaccionVM? RelacionGastosTransacciones { get; set; }
        public RelacionGasto? RelacionGasto { get; set; }
    }
}