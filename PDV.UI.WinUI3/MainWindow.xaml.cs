using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Services;
using PDV.UI.WinUI3.Views;
using System;

namespace PDV.UI.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private readonly ISyncService _syncService;
        private readonly ISyncNotificationService _syncNotificationService;

        public MainWindow()
        {
            this.InitializeComponent();

            NotificationService.Instance.Initialize(GlobalNotification);

            var app = Microsoft.UI.Xaml.Application.Current as App;
            var services = app?.Services;

            if (services != null)
            {
                _syncService = services.GetService<ISyncService>();
                _syncNotificationService = services.GetService<ISyncNotificationService>();

                if (_syncNotificationService != null)
                {
                    _syncNotificationService.SyncStatusChanged += SyncStatusChanged;
                }
            }

            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(HomePage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag);
            }
        }

        private void NavView_Navigate(string navItemTag)
        {
            Type pageType = navItemTag switch
            {
                "home" => typeof(HomePage),
                "employees" => typeof(EmployeesPage),
                "products" => typeof(ProductsPage),
                "reports" => typeof(ReportsPage),
                "pos" => typeof(POSPage),
                _ => typeof(HomePage)
            };

            ContentFrame.Navigate(pageType);
        }

        private async void SyncButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                NotificationService.Instance.ShowInformation("Iniciando sincronização...");
                await _syncService?.SynchronizeAsync();
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Erro na sincronização: {ex.Message}");
            }
        }

        private void SyncStatusChanged(object sender, SyncStatusEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                switch (e.Status)
                {
                    case PDV.Shared.Enum.SyncStatus.InProgress:
                        break;
                    case PDV.Shared.Enum.SyncStatus.Completed:
                        NotificationService.Instance.ShowSuccess(e.Message);
                        break;

                    case PDV.Shared.Enum.SyncStatus.Failed:
                        NotificationService.Instance.ShowError(e.Message);
                        break;
                }
            });
        }
    }
}