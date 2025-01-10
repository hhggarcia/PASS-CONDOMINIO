using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class PagoRecibidoVM: PagoRecibido
    {
        public FormaPago Pagoforma { get; set; }
        public int DeudaPagar { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? Propiedades { get; set; }
        public IList<SelectListItem>? RecibosModel { get; set; }        
        public decimal Saldo { get; set; }
        public decimal Interes { get; set; }
        public decimal Indexacion { get; set; }
        public decimal Credito { get; set; }
        public decimal Deuda { get; set; }
        public decimal Abonado { get; set; }
        public int IdRecibo { get; set; }
        public int IdPropiedad { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public IList<ReciboCobro>? Recibos { get; set; }
        public IList<SelectListItem>? ListRecibos { get; set; } = new List<SelectListItem>();
        public IList<int>? ListRecibosIDs { get; set; } = new List<int>();
    }
}
