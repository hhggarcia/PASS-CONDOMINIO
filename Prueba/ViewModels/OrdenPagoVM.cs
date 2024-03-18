using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class OrdenPagoVM: PagoEmitido
    {
        public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public FormaPago Pagoforma { get; set; }
        public int IdSubcuenta { get; set; }
        public int IdProveedor { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? Proveedor { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? ReferenciasDolar { get; set; }
    }
}
