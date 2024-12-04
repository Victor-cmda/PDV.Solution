using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PDV.Infrastructure.Services
{
    public class ConnectionManager
    {
        private readonly IConfiguration _configuration;
        private bool _isOnline;

        public ConnectionManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _isOnline = CheckConnection();
        }

        public bool IsOnline => _isOnline;

        public bool CheckConnection()
        {
            try
            {
                using var connection = new NpgsqlConnection(
                    _configuration.GetConnectionString("PostgresConnection"));
                connection.Open();
                _isOnline = true;
                return true;
            }
            catch
            {
                _isOnline = false;
                return false;
            }
        }
    }
}
