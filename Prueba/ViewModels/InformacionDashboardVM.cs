namespace Prueba.ViewModels
{
    public class InformacionDashboardVM
    {
        public decimal CuentasPorCobrar { get; set; }
        public decimal RecibosPendientes { get; set; }
        public decimal RecibosPagados { get; set; }
        public decimal RecibosNoPagados { get; set; }
        public decimal Deudores { get; set; }
        public decimal NoDeudores { get; set; }
        public Dictionary<int, decimal>? Ingresos { get; set; }
        public Dictionary<int, decimal>? Egresos { get; set; }
    }
}
