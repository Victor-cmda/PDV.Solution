namespace PDV.Domain.Interfaces
{
    namespace PDV.Domain.Interfaces
    {
        public interface IConnectivityService
        {
            bool IsOnline();
            Task CheckAndUpdateConnectivityAsync();
        }
    }
}
