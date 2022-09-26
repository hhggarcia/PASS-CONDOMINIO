using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class RegistroPagoVM: PagoEmitido
    {
        public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public int IdCodigoCuenta { get; set; }
        public TipoOperacion TipoOperacion { get; set; }
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public int IdSubcuenta { get; set; }
        public int NumReferencia { get; set; }
        public string? Banco { get; set; }

    }
}
