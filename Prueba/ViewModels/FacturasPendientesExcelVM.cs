using System.ComponentModel.DataAnnotations;

namespace Prueba.ViewModels
{
    public class FacturasPendientesExcelVM
    {
        public string Factura { get; set; } = string.Empty;

        [Display(Name = "Base Imponible")]
        public decimal Base { get; set; }

        [Display(Name = "IVA 16%")]
        public decimal Iva { get; set; }

        [Display(Name = "Monto Total")]
        public decimal MontoTotal { get; set; }

        [Display(Name = "RET. IVA")]
        public decimal RetIva { get; set; }
        [Display(Name = "RET. ISLR")]
        public decimal RetIslr { get; set; }
        [Display(Name = "Total a Pagar")]
        public decimal TotalPagar { get; set; }
        [Display(Name = "Pago Recibido")]
        public decimal PagoRecibido { get; set; }
        [Display(Name = "Pendiente a Pagar")]
        public decimal PendientePago { get; set; }
    }
}
