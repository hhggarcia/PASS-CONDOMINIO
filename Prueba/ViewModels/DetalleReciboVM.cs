using Prueba.Areas.Identity.Data;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class DetalleReciboVM
    {
        public ReciboCobro Recibo { get; set; } = new ReciboCobro();
        public RelacionDeGastosVM RelacionGastos { get; set; } = new RelacionDeGastosVM();
        public Propiedad Propiedad { get; set; } = new Propiedad();
        public AspNetUser Propietario { get; set; } = new AspNetUser();
        public decimal Total { get; set; } = 0;
        public decimal SaldoPagar { get; set; } = 0;
    }
}
