namespace Prueba.ViewModels
{
    public class AsignarCobroTransitoFacturaVM
    {
        public int IdFactura { get; set; }
        public int IdCobroTransito { get; set; }
        public bool RetIva { get; set; }
        public DateTime FechaEmisionRetIva { get; set; }
        public string? NumComprobanteRetIva { get; set; }
        public bool RetIslr { get; set; }
        public DateTime FechaEmisionIslr { get; set; }
        public string? NumComprobanteRetIslr { get; set; }
    }
}