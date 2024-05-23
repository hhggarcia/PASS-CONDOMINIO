using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobantePEVM
    {
        public Condominio? Condominio { get; set; }
        public PagoEmitido Pago { get; set; } = new PagoEmitido();
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
        public Factura Factura { get; set; }

        public Anticipo? Anticipo { get; set; }
        public string Beneficiario { get; set; }
        public bool retencionesIva { get; set; }
        public bool retencionesIslr { get; set; }
    }
}
