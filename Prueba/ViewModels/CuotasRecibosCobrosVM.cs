using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CuotasRecibosCobrosVM
    {
        public CuotasEspeciale CuotasEspeciale { get; set; }=new CuotasEspeciale();
        public ReciboCuota ReciboCuota { get; set; } = new ReciboCuota();
    }
}
