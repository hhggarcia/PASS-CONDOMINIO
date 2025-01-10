using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class PagoRecibidoCuotaVM : PagoRecibido
    {
        public int idRecibo {  get; set; }
        public ReciboCuota ReciboCuotas { get; set; }=new ReciboCuota();
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
    }
}
