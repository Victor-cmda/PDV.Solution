using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Domain.Interfaces;
using PDV.Domain.Interfaces.PDV.Domain.Interfaces;
using PDV.Shared.Enum;
using System;
using System.Threading.Tasks;

namespace PDV.UI.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private readonly ISyncService _syncService;
        private readonly ISyncNotificationService _syncNotificationService;
        private readonly IConnectivityService _connectivityService;
        private DispatcherTimer _infoBarTimer;
        public MainWindow()
        {
            this.InitializeComponent();

            var serviceProvider = ((App)Microsoft.UI.Xaml.Application.Current).Services;

            _syncService = serviceProvider.GetService<ISyncService>();
            _syncNotificationService = serviceProvider.GetService<ISyncNotificationService>();
            _connectivityService = serviceProvider.GetService<IConnectivityService>();

            _infoBarTimer = new DispatcherTimer();
            _infoBarTimer.Interval = TimeSpan.FromSeconds(5);
            _infoBarTimer.Tick += InfoBarTimer_Tick;

            if (_syncNotificationService != null)
            {
                _syncNotificationService.SyncStatusChanged += SyncNotificationService_SyncStatusChanged;
            }

            SyncInfoBar.IsClosable = true;
            SyncInfoBar.CloseButtonClick += SyncInfoBar_CloseButtonClick;

            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void InfoBarTimer_Tick(object sender, object e)
        {
            SyncInfoBar.IsOpen = false;
            _infoBarTimer.Stop();
        }

        private void SyncInfoBar_CloseButtonClick(InfoBar sender, object args)
        {
            _infoBarTimer.Stop();
            SyncInfoBar.IsOpen = false;
        }

        private void SyncNotificationService_SyncStatusChanged(object sender, SyncStatusEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                SyncInfoBar.IsOpen = true;
                SyncInfoBar.Message = e.Message;

                switch (e.Status)
                {
                    case SyncStatus.InProgress:
                        SyncInfoBar.Severity = InfoBarSeverity.Informational;
                        break;
                    case SyncStatus.Completed:
                        SyncInfoBar.Severity = InfoBarSeverity.Success;
                        _infoBarTimer.Start();
                        break;
                    case SyncStatus.Failed:
                        SyncInfoBar.Severity = InfoBarSeverity.Error;
                        _infoBarTimer.Start();
                        break;
                }
            });
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();

                switch (navItemTag)
                {
                    case "home":
                        ContentFrame.Navigate(typeof(Views.HomePage));
                        break;
                    case "employees":
                        ContentFrame.Navigate(typeof(Views.EmployeesPage));
                        break;
                    case "products":
                        ContentFrame.Navigate(typeof(Views.ProductsPage));
                        break;
                    case "reports":
                        ContentFrame.Navigate(typeof(Views.ReportsPage));
                        break;
                    case "pos":
                        ContentFrame.Navigate(typeof(Views.POSPage));
                        break;
                }
            }
        }
        private async void SyncButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await SyncDataAsync();
        }

        private async Task SyncDataAsync()
        {
            if (_syncService == null) return;

            try
            {
                await _syncService.SynchronizeAsync();
            }
            catch (Exception ex)
            {
            }
        }

        private void OnSyncStatusChanged(object sender, SyncStatusEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                SyncInfoBar.IsOpen = true;
                SyncInfoBar.Message = e.Message;

                switch (e.Status)
                {
                    case SyncStatus.InProgress:
                        SyncInfoBar.Severity = InfoBarSeverity.Informational;
                        break;
                    case SyncStatus.Completed:
                        SyncInfoBar.Severity = InfoBarSeverity.Success;
                        break;
                    case SyncStatus.Failed:
                        SyncInfoBar.Severity = InfoBarSeverity.Error;
                        break;
                }
            });
        }
    }
}
