using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class RegistroPagoVM: PagoEmitido
    {
        public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public int IdCodigoCuenta { get; set; }
        public int TipoOperacion { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public int IdSubcuenta { get; set; }

    }
}
