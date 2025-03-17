using Microsoft.Extensions.Logging;
using PDV.Domain.Interfaces.PDV.Domain.Interfaces;

namespace PDV.Infrastructure.Services
{
    public class ConnectivityService : IConnectivityService
    {
        private readonly ILogger<ConnectivityService> _logger;
        private bool _isOnline;

        public ConnectivityService(ILogger<ConnectivityService> logger)
        {
            _logger = logger;
            _isOnline = CheckConnectivity();
        }

        public bool IsOnline() => _isOnline;

        public async Task CheckAndUpdateConnectivityAsync()
        {
            _isOnline = CheckConnectivity();
            _logger.LogInformation($"Connectivity status: {(_isOnline ? "Online" : "Offline")}");
        }

        private bool CheckConnectivity()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.GetAsync("https://www.google.com").Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
