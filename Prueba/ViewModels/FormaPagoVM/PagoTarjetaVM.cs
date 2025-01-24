namespace Prueba.ViewModels.FormaPagoVM
{
    public class PagoTarjetaVM
    {
        public string NumeroTarjeta { get; set; } = string.Empty;
        public string NombreTitular { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public string CVV { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
