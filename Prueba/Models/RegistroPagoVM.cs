using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models
{
    public class RegistroPagoVM: PagoEmitido
    {
        public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? ReferenciasDolar { get; set; }
        public int IdSubcuenta { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        //public int IdReferenciaDolar { get; set; }
        public int IdMonedaCond { get; set; }

    }
}
