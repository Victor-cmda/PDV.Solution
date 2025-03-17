using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3.Controls
{
    public sealed partial class SyncStatusIndicator : UserControl
    {
        private readonly ISyncService _syncService;
        private readonly Timer _timer;

        public SyncStatusIndicator()
        {
            this.InitializeComponent();

            // Obter serviço
            _syncService = ((App)Microsoft.UI.Xaml.Application.Current).Services.GetService<ISyncService>();

            // Configurar timer para verificar pendências
            _timer = new Timer(30000); // 30 segundos
            _timer.Elapsed += async (s, e) => await CheckPendingSyncItemsAsync();
            _timer.Start();

            // Verificar status inicial
            _ = CheckPendingSyncItemsAsync();
        }

        private async Task CheckPendingSyncItemsAsync()
        {
            if (_syncService == null) return;

            try
            {
                var hasPendingItems = await _syncService.HasPendingSyncItemsAsync();

                DispatcherQueue.TryEnqueue(() =>
                {
                    PendingChangesIndicator.Visibility = hasPendingItems ?
                        Visibility.Visible : Visibility.Collapsed;
                });
            }
            catch (Exception)
            {
                // Ignorar erros
            }
        }
    }
}
