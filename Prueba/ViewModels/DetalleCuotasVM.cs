using Prueba.Models;

namespace Prueba.ViewModels
{
    public class DetalleCuotasVM
    {
        public CuotasEspeciale CuotasEspeciale { get; set; }= new CuotasEspeciale();
        public decimal TotalPropiedadesMensual { get; set; } = 0;
    }
}
