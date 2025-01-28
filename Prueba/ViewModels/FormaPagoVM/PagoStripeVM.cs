namespace Prueba.ViewModels.FormaPagoVM
{
    public class PagoStripeVM
    {
        public string Token { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
