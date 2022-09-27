using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class RegistroPagoVM
    {
        public int IdCondominio { get; set; }
        public int? IdProveedor { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public bool FormaPago { get; set; }
        public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public int IdCodigoCuentaGasto { get; set; }
        public TipoOperacion TipoOperacion { get; set; }
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public int IdSubcuenta { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }

    }
}
