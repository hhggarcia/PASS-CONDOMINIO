namespace Prueba.Services
{
    public interface IEpaymentsBnc
    {

    }
    public class EpaymentsBnc : IEpaymentsBnc
    {
        private readonly HttpClient _httpClient;

        public EpaymentsBnc(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
