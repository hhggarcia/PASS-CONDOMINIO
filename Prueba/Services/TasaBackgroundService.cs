using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NCrontab;
using Newtonsoft.Json;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Utils;
using SQLitePCL;

namespace Prueba.Services
{
    public class TasaBackgroundService : IHostedService, IDisposable
    {
        private readonly ILogger<TasaBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<CronJobSettings> _cronSettingsMonitor;
        private Timer _timer;
        private CrontabSchedule _schedule1;
        private CrontabSchedule _schedule2;
        private DateTime _nextRun1;
        private DateTime _nextRun2;

        public TasaBackgroundService(
            ILogger<TasaBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<CronJobSettings> cronSettingsMonitor)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _cronSettingsMonitor = cronSettingsMonitor;

            LoadSchedules();
        }

        private void LoadSchedules()
        {
            var cronSettings = _cronSettingsMonitor.CurrentValue;

            if (cronSettings == null)
            {
                throw new InvalidOperationException("Configuración de CronJobs no encontrada");
            }

            _schedule1 = CrontabSchedule.Parse(cronSettings.TasaJob1 ?? "0 7 * * *"); 
            _schedule2 = CrontabSchedule.Parse(cronSettings.TasaJob2 ?? "0 16 * * *");
            _nextRun1 = _schedule1.GetNextOccurrence(DateTime.UtcNow);
            _nextRun2 = _schedule2.GetNextOccurrence(DateTime.UtcNow);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de tasas iniciado.");

            // Verificamos cada minuto si es hora de ejecutar
            // _timer = new Timer(async _ => await ExecuteJobAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _timer = new Timer(CheckSchedules, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void CheckSchedules(object state)
        {
            var now = DateTime.UtcNow;

            if (now >= _nextRun1)
            {
                ExecuteJobAsync().ConfigureAwait(false);
                _nextRun1 = _schedule1.GetNextOccurrence(now);
            }

            if (now >= _nextRun2)
            {
                ExecuteJobAsync().ConfigureAwait(false);
                _nextRun2 = _schedule2.GetNextOccurrence(now);
            }
        }

        private async Task ExecuteJobAsync()
        {
            _logger.LogInformation($"Iniciando actualización de tasas a las {DateTime.UtcNow:u}");

            using var scope = _scopeFactory.CreateScope();
            try
            {
                var exchangeService = scope.ServiceProvider.GetRequiredService<IExchangeApiService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<NuevaAppContext>();

                await ProcessExchangeRatesAsync(exchangeService, dbContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la actualización de tasas");
            }
        }

        private async Task ProcessExchangeRatesAsync(IExchangeApiService exchangeService, NuevaAppContext dbContext)
        {
            var jsonResponse = await exchangeService.Convertion("USD", "VES");
            var result = JsonConvert.DeserializeObject<ResponseSuccess>(jsonResponse);

            if (result == null)
            {
                _logger.LogWarning("La API devolvió una respuesta nula");
                return;
            }

            try
            {
                var ultimaTasa = await dbContext.HistorialMoneda.FirstOrDefaultAsync(c => c.Actual);
                var moneda = await dbContext.Moneda.FirstOrDefaultAsync(c => c.Codigo == "VES");

                if (moneda == null)
                {
                    _logger.LogWarning("No existe moneda registrada para VES");
                    return;
                }

                if (ultimaTasa != null)
                {
                    ultimaTasa.Actual = false;
                    dbContext.HistorialMoneda.Update(ultimaTasa);
                }

                var nuevaTasa = new HistorialMoneda()
                {
                    IdMoneda = moneda.IdMoneda,
                    BaseCode = result.Base_Code,
                    TargetCode = result.Target_Code,
                    ConversionRate = result.Conversion_Rate,
                    ConversionResult = result.Conversion_Rate,
                    FechaConsulta = DateTime.UtcNow,
                    Actual = true
                };               
               
                dbContext.HistorialMoneda.Add(nuevaTasa);

                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Tasa actualizada: {result.Conversion_Rate}");
            }
            catch
            {
                _logger.LogInformation($"Error al actualizar la tasa en base de datos");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de tasas detenido.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

    public class ResponseSuccess
    {
        public string Result { get; set; } = string.Empty;
        public string Documentation { get; set; } = string.Empty;
        public string Terms_Of_Use { get; set; } = string.Empty;
        public string Time_Last_Update_Unix { get; set; } = string.Empty;
        public string Time_Last_Update_Utc { get; set; } = string.Empty;
        public string Time_Next_Update_Unix { get; set; } = string.Empty;
        public string Time_Next_Update_Utc { get; set; } = string.Empty;
        public string Base_Code { get; set; } = string.Empty;
        public string Target_Code { get; set; } = string.Empty; 
        public decimal Conversion_Rate { get; set; }
    }
}
