using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobantePagoNomina
    {
        public Condominio? Condominio { get; set; }
        public PagoEmitido Pago { get; set; } = new PagoEmitido();
        public Empleado? Empleado { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public FormaPago Pagoforma { get; set; }
        public SubCuenta? Gasto { get; set; }
        public int NumReferencia { get; set; }
        public decimal ValorDolar { get; set; }
        public SubCuenta? Banco { get; set; }
        public SubCuenta? Caja { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public decimal MontoRef { get; set; }
        public string SimboloMoneda { get; set; } = null!;
        public string SimboloRef { get; set; } = null!;
        public IList<Percepcion> Percepciones { get; set; } = new List<Percepcion>();
        public IList<Deduccion> Deducciones { get; set; } = new List<Deduccion>();
        public IList<Bonificacion> Bonos { get; set; } = new List<Bonificacion>();
    }
}
