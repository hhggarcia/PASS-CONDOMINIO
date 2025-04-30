using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NCrontab;
using Newtonsoft.Json;
using Prueba.Context;
using Prueba.Models;
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

            _schedule1 = CrontabSchedule.Parse(cronSettings.TasaJob1 ?? "0 9 * * *"); // Valor por defecto
            _schedule2 = CrontabSchedule.Parse(cronSettings.TasaJob2 ?? "0 17 * * *"); // Valor por defecto
            _nextRun1 = _schedule1.GetNextOccurrence(DateTime.UtcNow);
            _nextRun2 = _schedule2.GetNextOccurrence(DateTime.UtcNow);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de tasas iniciado.");

            // Verificamos cada minuto si es hora de ejecutar
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

            var ultimaTasa = await dbContext.HistorialMoneda.FirstOrDefaultAsync(c => c.Actual);
            if (ultimaTasa == null)
            {
                _logger.LogWarning("No se encontró tasa activa en la base de datos");
                return;
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                ultimaTasa.Actual = false;

                var nuevaTasa = new HistorialMoneda()
                {
                    IdMoneda = ultimaTasa.IdMoneda,
                    BaseCode = result.BaseCode,
                    TargetCode = result.TargetCode,
                    ConversionRate = result.ConversionRate,
                    ConversionResult = result.ConversionRate,
                    FechaConsulta = DateTime.UtcNow,
                    Actual = true
                };

                dbContext.HistorialMoneda.Update(ultimaTasa);
                dbContext.HistorialMoneda.Add(nuevaTasa);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Tasa actualizada: {result.ConversionRate}");
            }
            catch
            {
                await transaction.RollbackAsync();
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
        public string Result { get; set; }
        public string BaseCode { get; set; }
        public string TargetCode { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
