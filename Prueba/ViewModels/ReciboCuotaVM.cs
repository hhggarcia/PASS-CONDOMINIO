using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ReciboCuotaVM
    {
        public string? Nombre { get; set; }

        public ReciboCuota ReciboCuota { get; set; }=new ReciboCuota();
    }
}
