using Prueba.Models;

namespace Prueba.ViewModels
{
    public class DetalleReciboCuotasVM
    {
        public ReciboCuota reciboCuota { get; set; } = new ReciboCuota();
        public CuotasEspeciale cuotasEspeciale { get; set; }  = new CuotasEspeciale();
    }
}
