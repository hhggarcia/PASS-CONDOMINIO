using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CobrarCuotasVM
    {
        public string? NombreUsuario { get; set; }
        public string? CodigoPropiedad { get; set; }
        public PagoRecibido PagoReciboCuota { get; set; } = new PagoRecibido();
        //public PagoReciboCuota PagoReciboCuota { get; set; } = new PagoReciboCuota();
        public CuotasEspeciale CuotasEspeciale { get; set; } = new CuotasEspeciale();
    }
}
