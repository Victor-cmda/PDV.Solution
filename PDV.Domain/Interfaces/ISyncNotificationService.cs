using PDV.Shared.Enum;

namespace PDV.Domain.Interfaces
{
    public interface ISyncNotificationService
    {
        event EventHandler<SyncStatusEventArgs> SyncStatusChanged;
        void NotifySyncStarted();
        void NotifySyncCompleted();
        void NotifySyncFailed(string errorMessage);
    }

    public class SyncStatusEventArgs : EventArgs
    {
        public SyncStatus Status { get; set; }
        public string Message { get; set; }
    }
}
