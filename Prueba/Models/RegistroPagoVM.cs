using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class RegistroPagoVM
    {
        public int IdCondominio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
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
        public int IdReferenciaDolar { get; set; }

    }
}
