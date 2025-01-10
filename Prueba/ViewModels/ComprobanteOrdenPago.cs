using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobanteOrdenPago
    {
        public Condominio? Condominio { get; set; }
        public PagoEmitido Pago { get; set; } = new PagoEmitido();
        public string Concepto { get; set; } = string.Empty;
        public FormaPago Pagoforma { get; set; }
        public SubCuenta? Gasto { get; set; }
        public int NumReferencia { get; set; }
        public decimal ValorDolar { get; set; }
        public SubCuenta? Banco { get; set; } = new SubCuenta();
        public SubCuenta? Caja { get; set; } = new SubCuenta();
        public string Mensaje { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public bool RelacionGasto { get; set; }

    }
}
