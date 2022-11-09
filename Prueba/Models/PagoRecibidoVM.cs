using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class PagoRecibidoVM
    {
        public int IdCondominio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        //public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public FormaPago Pagoforma { get; set; }
        public int DeudaPagar { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? Propiedades { get; set; }
        //public IList<SelectListItem>? RecibosModel { get; set; }
        public decimal Saldo { get; set; }
        public decimal Deuda { get; set; } 
        public IList<ReciboCobro>? Recibos { get; set; }
        public int IdPropiedad { get; set; }
        //public int IdRecibo { get; set; }
        public int IdSubcuenta { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
    }
}
