using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PDV.Infrastructure.Services
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SyncBackgroundService> _logger;

        public SyncBackgroundService(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger<SyncBackgroundService> logger)
        {
            _services = services;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var syncService = scope.ServiceProvider.GetRequiredService<SyncService>();
                    var connectionManager = scope.ServiceProvider.GetRequiredService<ConnectionManager>();

                    if (connectionManager.IsOnline)
                    {
                        await syncService.SynchronizeAsync();
                        _logger.LogInformation("Sincronização concluída com sucesso");
                    }

                    // Obter o intervalo de configuração
                    var intervalStr = _configuration.GetSection("SyncSettings:AutoSyncInterval").Value;
                    var interval = TimeSpan.Parse(intervalStr ?? "00:05:00");

                    await Task.Delay(interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante a sincronização automática");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
