using PDV.Domain.Interfaces;
using PDV.Shared.Enum;

namespace PDV.Application.Services
{
    public class SyncNotificationService : ISyncNotificationService
    {
        public event EventHandler<SyncStatusEventArgs> SyncStatusChanged;

        public void NotifySyncStarted()
        {
            SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs
            {
                Status = SyncStatus.InProgress,
                Message = "Sincronização em andamento..."
            });
        }

        public void NotifySyncCompleted()
        {
            SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs
            {
                Status = SyncStatus.Completed,
                Message = "Sincronização concluída com sucesso"
            });
        }

        public void NotifySyncFailed(string errorMessage)
        {
            SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs
            {
                Status = SyncStatus.Failed,
                Message = $"Falha na sincronização: {errorMessage}"
            });
        }
    }
}
