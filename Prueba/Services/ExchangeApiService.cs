using NPOI.SS.Formula.Functions;
using Prueba.Context;
using Prueba.Utils;
using System.Text;
using System.Text.Json;

namespace Prueba.Services
{
    public interface IExchangeApiService
    {
        Task<string> Convertion(string countryBase, string countryTarget);
    }

    public class ExchangeApiService : IExchangeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeApiService> _logger;
        private readonly IServiceProvider _serviceProvider;


        public ExchangeApiService(IServiceProvider serviceProvider,
            HttpClient httpClient,
            ILogger<ExchangeApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> Convertion(string countryBase, string countryTarget)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var _dataApi = scope.ServiceProvider.GetRequiredService<ExchangeData>();


                var endpoint = _dataApi.BaseUrl + _dataApi.ApiKey + "/pair/" + countryBase + "/" + countryTarget;

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a la API");
                return $"Error: " + ex.Message;
            }          

        }
    }
}
