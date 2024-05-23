using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class PagoFacturaEmitidaVM: PagoRecibido
    {
        public string? Descripcion { get; set; }
        public FormaPago Pagoforma { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? Clientes { get; set; }
        public IList<SelectListItem>? Facturas { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? ReferenciasDolar { get; set; }
        public int IdSubcuenta { get; set; }
        public int IdCliente { get; set; }
        public int IdFactura { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public decimal RetIva { get; set; }
        public decimal RetIslr { get; set; }
        public bool RetencionesIva { get; set; }
        public bool RetencionesIslr { get; set; }
        public DateTime FechaEmisionRetIva { get; set; }
        public string NumComprobanteRetIva { get; set; } = string.Empty;
        public DateTime FechaEmisionIslr { get; set; }
        public string NumComprobanteRetIslr { get; set; } = string.Empty;
    }
}
