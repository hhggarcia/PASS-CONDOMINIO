using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobanteAnticipoVM
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
        public string Beneficiario { get; set; } = string.Empty;
    }
}
