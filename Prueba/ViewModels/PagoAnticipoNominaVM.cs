using Prueba.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.ViewModels
{
    public class PagoAnticipoNominaVM: PagoEmitido
    {
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? Empleados { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public int IdSubcuenta { get; set; }
        public int IdEmpleado { get; set; }
    }
}
