namespace Prueba.ViewModels
{
    public class DeudoresDiarioVM
    {
        public string Codigo { get; set; } = string.Empty;
        public string Propietario { get; set; } = string.Empty;
        public int CantRecibos { get; set; }
        public decimal AcumDeuda { get; set; }
        public decimal AcumMora { get; set; }
        public decimal AcumIndexacion { get; set; }
        public decimal Credito { get; set; }
        public decimal Saldo { get; set; }
        public decimal Total { get; set; }
    }
}
