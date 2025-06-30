namespace Prueba.ViewModels
{
    public class CuentasCobrarExcelVM
    {
        public string Empresa { get; set; }
        public int FacturasPendientes { get; set; }
        public string RetencionesPendientesIva { get; set; }
        public string RetencionesPendientesIslr { get; set; }
        public decimal TotalPagar { get; set; }
        public decimal TotalPagarRef { get; set; }
    }
}
