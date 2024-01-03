using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CobrarCuotasVM
    {
        public string? NombreUsuario { get; set; }
        public string? CodigoPropiedad { get; set; }
        public PagoReciboCuota PagoReciboCuota { get; set; } = new PagoReciboCuota();
        public CuotasEspeciale CuotasEspeciale { get; set; } = new CuotasEspeciale();
    }
}
