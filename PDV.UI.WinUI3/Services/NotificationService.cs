using PDV.UI.WinUI3.Controls;

namespace PDV.UI.WinUI3.Services
{
    public class NotificationService
    {
        private static NotificationService _instance;
        private NotificationControl _notificationControl;

        private NotificationService() { }

        public static NotificationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NotificationService();
                }
                return _instance;
            }
        }

        public void Initialize(NotificationControl notificationControl)
        {
            _notificationControl = notificationControl;
        }

        public void ShowInformation(string message)
        {
            if (_notificationControl == null) return;
            _notificationControl.ShowNotification(message, NotificationType.Information);
        }

        public void ShowSuccess(string message)
        {
            if (_notificationControl == null) return;
            _notificationControl.ShowNotification(message, NotificationType.Success);
        }

        public void ShowWarning(string message)
        {
            if (_notificationControl == null) return;
            _notificationControl.ShowNotification(message, NotificationType.Warning);
        }

        public void ShowError(string message)
        {
            if (_notificationControl == null) return;
            _notificationControl.ShowNotification(message, NotificationType.Error);
        }
    }
}