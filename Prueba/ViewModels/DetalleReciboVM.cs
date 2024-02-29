using Prueba.Areas.Identity.Data;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class DetalleReciboVM
    {
        public ReciboCobro Recibo { get; set; } = new ReciboCobro();
        //public List<ReciboCuota>  ReciboCuota { get; set; } = new List<ReciboCuota>();
        public RelacionDeGastosVM RelacionGastos { get; set; } = new RelacionDeGastosVM();
        //public List<CuotasEspeciale> CuotasEspeciale { get; set; }=new List<CuotasEspeciale>();
        public List<CuotasRecibosCobrosVM> CuotasRecibosCobros { get; set; } = new List<CuotasRecibosCobrosVM> ();
        public Propiedad Propiedad { get; set; } = new Propiedad();
        public AspNetUser Propietario { get; set; } = new AspNetUser();
        public decimal Total { get; set; } = 0;
        public decimal SaldoPagar { get; set; } = 0;
        public int IdDetalleRecibo { get; set; } = 0;
    }
}
