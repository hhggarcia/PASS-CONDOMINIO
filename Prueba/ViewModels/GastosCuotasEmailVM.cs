using Prueba.Models;

namespace Prueba.ViewModels
{
    public class GastosCuotasEmailVM
    {
        public ReciboCobro ReciboCobro { get; set; }
        public CuotasEspeciale CuotasEspeciale { get; set; }
        public Propiedad Propiedad { get; set; }
        public string Email { get; set; }
    }
}
