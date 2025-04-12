using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

namespace PDV.UI.WinUI3.Controls
{
    public sealed partial class NotificationControl : UserControl
    {
        private readonly Queue<NotificationItem> _pendingNotifications = new Queue<NotificationItem>();
        private bool _isShowingNotification = false;
        private DispatcherTimer _autoCloseTimer;

        public NotificationControl()
        {
            this.InitializeComponent();

            _autoCloseTimer = new DispatcherTimer();
            _autoCloseTimer.Interval = TimeSpan.FromSeconds(4);
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
        }

        private void AutoCloseTimer_Tick(object sender, object e)
        {
            _autoCloseTimer.Stop();
            DismissCurrentNotification();
        }

        public void ShowNotification(string message, NotificationType type = NotificationType.Information)
        {
            var notificationItem = new NotificationItem { Message = message, Type = type };
            _pendingNotifications.Enqueue(notificationItem);

            if (!_isShowingNotification)
            {
                ShowNextNotification();
            }
        }

        private async void ShowNextNotification()
        {
            if (_pendingNotifications.Count == 0)
            {
                RootGrid.Visibility = Visibility.Collapsed;
                _isShowingNotification = false;
                return;
            }

            _isShowingNotification = true;
            RootGrid.Visibility = Visibility.Visible;

            var notificationToShow = _pendingNotifications.Dequeue();
            CreateNotificationPanel(notificationToShow);

            PopupStoryboard.Begin();
            _autoCloseTimer.Start();
        }

        private void CreateNotificationPanel(NotificationItem item)
        {
            NotificationsContainer.Children.Clear();

            var panel = new Grid()
            {
                Name = "NotificationPanel",
                Opacity = 0,
                RenderTransform = new TranslateTransform()
            };

            var infoBar = new InfoBar()
            {
                Title = GetNotificationTitle(item.Type),
                Message = item.Message,
                IsOpen = true,
                Severity = GetSeverity(item.Type),
                IsClosable = true
            };

            infoBar.CloseButtonClick += (s, e) =>
            {
                _autoCloseTimer.Stop();
                DismissCurrentNotification();
            };

            panel.Children.Add(infoBar);
            NotificationsContainer.Children.Add(panel);
        }

        private void DismissCurrentNotification()
        {
            if (NotificationsContainer.Children.Count > 0)
            {
                DismissStoryboard.Begin();
            }
        }

        private void DismissStoryboard_Completed(object sender, object e)
        {
            NotificationsContainer.Children.Clear();
            ShowNextNotification();
        }

        private InfoBarSeverity GetSeverity(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => InfoBarSeverity.Success,
                NotificationType.Warning => InfoBarSeverity.Warning,
                NotificationType.Error => InfoBarSeverity.Error,
                _ => InfoBarSeverity.Informational,
            };
        }

        private string GetNotificationTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "Sucesso",
                NotificationType.Warning => "Atenção",
                NotificationType.Error => "Erro",
                _ => "Informação",
            };
        }
    }

    public class NotificationItem
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
    }

    public enum NotificationType
    {
        Information,
        Success,
        Warning,
        Error
    }
}