using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Domain.Interfaces;
using PDV.Shared.Enum;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly ISyncNotificationService _syncNotificationService;
        private readonly ISyncService _syncService;
        public MainWindow()
        {
            this.InitializeComponent();

            // Obter serviços
            _syncNotificationService = ((App)Microsoft.UI.Xaml.Application.Current).Services.GetService<ISyncNotificationService>();
            _syncService = ((App)Microsoft.UI.Xaml.Application.Current).Services.GetService<ISyncService>();

            // Assinar eventos
            if (_syncNotificationService != null)
            {
                _syncNotificationService.SyncStatusChanged += OnSyncStatusChanged;
            }

            // Inicializar navegação
            NavView.SelectedItem = NavView.MenuItems[0];
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
                // A notificação de falha já foi tratada pelo serviço de sincronização
                // Podemos adicionar tratamento adicional aqui, se necessário
            }
        }

        private void OnSyncStatusChanged(object sender, SyncStatusEventArgs e)
        {
            // Como estamos em uma thread diferente, precisamos usar o dispatcher
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
