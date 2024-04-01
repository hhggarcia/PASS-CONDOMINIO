using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CobroTransitoVM: PagoRecibido
    {
        public string? Descripcion { get; set; }
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public int IdSubcuenta { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public bool Factura { get; set; }
        public bool Recibo { get; set; }

    }
}
