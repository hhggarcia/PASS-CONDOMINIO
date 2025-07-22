using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ReciboPagadoVM
    {
        public ReciboCobro? Recibo { get; set; }
        public Propiedad? Propiedad { get; set; }
        public List<PagoRecibido> Pago { get; set; } = new List<PagoRecibido>();
    }
}
