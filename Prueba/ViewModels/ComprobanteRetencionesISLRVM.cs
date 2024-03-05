using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobanteRetencionesISLRVM
    {
        public ComprobanteRetencion comprobanteRetencion { get; set; }
        public string NumComprobante {  get; set; }
        public string NumControl {  get; set; }
        public string NumFactura { get; set; }
        public Condominio Condominio { get; set; }
        public Proveedor Proveedor { get; set; }
    }
}
