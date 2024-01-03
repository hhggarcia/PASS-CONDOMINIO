using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ComprobanteCEVM
    {
        public Condominio? Condominio { get; set; }
        public CuotasEspeciale CuotasEspeciale { get; set; }
        public PagoReciboCuota PagoReciboCuota { get; set; }
        public ReciboCuota ReciboCuota { get; set; }
        public Propiedad? Propiedad { get; set; }
        public ReferenciasPr? Referencias { get; set; }
        public DateTime FechaComprobante { get; set; }
        public decimal Restante { get; set; }
        public decimal Abonado { get; set; }
        public string? Mensaje { get; set; }
    }
}
