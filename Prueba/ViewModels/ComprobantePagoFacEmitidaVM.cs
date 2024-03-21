using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobantePagoFacEmitidaVM
    {
        public Condominio? Condominio { get; set; }
        public PagoRecibido Pago { get; set; } = new PagoRecibido();
        public string Concepto { get; set; } = string.Empty;
        public FormaPago Pagoforma { get; set; }
        public SubCuenta? Gasto { get; set; }
        public int NumReferencia { get; set; }
        public decimal ValorDolar { get; set; }
        public SubCuenta? Banco { get; set; }
        public SubCuenta? Caja { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public decimal Islr { get; set; }
        public decimal Iva { get; set; }
        public FacturaEmitida Factura { get; set; }
        public string Beneficiario { get; set; }
        public bool RetencionesIva { get; set; }
        public bool RetencionesIslr { get; set; }
    }
}
