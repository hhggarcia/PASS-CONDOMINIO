namespace Prueba.ViewModels
{
    public class CuentasPagarVM
    {
        public string Condominio { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string NumFactura { get; set; } = string.Empty;
        public decimal BaseImponible { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal Iva { get; set; }
        public decimal RetIva { get; set; }
        public decimal RetIslr { get; set; }
        public decimal TotalPagar { get; set; }
    }
}
