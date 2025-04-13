using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Services;
using PDV.UI.WinUI3.Views;
using System;
using System.Collections.Generic;

namespace PDV.UI.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private readonly ISyncService _syncService;
        private readonly ISyncNotificationService _syncNotificationService;
        private DispatcherTimer _clockTimer;
        private Dictionary<string, string> _pageTitles = new Dictionary<string, string>
        {
            { "home", "Dashboard" },
            { "employees", "Funcion�rios" },
            { "products", "Produtos" },
            { "reports", "Relat�rios" },
            { "pos", "PDV - Vendas" },
            { "sync", "Sincroniza��o" },
            { "settings", "Configura��es" }
        };

        public MainWindow()
        {
            this.InitializeComponent();

            // Configurar o servi�o de notifica��o
            NotificationService.Instance.Initialize(GlobalNotification);


            // Obter servi�os
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

            // Inicializar a navega��o
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(HomePage));

            // Inicializar o rel�gio
            InitializeClock();

            // Definir t�tulo personalizado da janela
            Title = "Sistema PDV - Gest�o Completa";

            // Configura��es adicionais da janela
            SetupWindow();
        }

        private void SetupWindow()
        {
            // Configurar tamanho da janela (exemplo)
            var appWindow = this.AppWindow;
            if (appWindow != null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32(1280, 800));
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(
                    (1920 - 1280) / 2, // Centralizar na tela horizontalmente
                    (1080 - 800) / 2,  // Centralizar na tela verticalmente
                    1280, 800));
            }
        }

        private void InitializeClock()
        {
            // Atualizar a data inicial
            UpdateDateTime();

            // Criar timer para atualizar o rel�gio
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(30); // Atualiza a cada 30 segundos
            _clockTimer.Tick += (s, e) => UpdateDateTime();
            _clockTimer.Start();
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;
            CurrentDateText.Text = now.ToString("dd MMMM yyyy");
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();

                // Atualizar o t�tulo da p�gina
                if (_pageTitles.ContainsKey(navItemTag))
                {
                    PageTitle.Text = _pageTitles[navItemTag];
                }

                // Navegar para a p�gina selecionada
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
                "settings" => typeof(HomePage),
                _ => typeof(HomePage)
            };

            // S� navega se for uma p�gina diferente da atual
            if (ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }

        private async void SyncButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                NotificationService.Instance.ShowInformation("Iniciando sincroniza��o...");
                await _syncService?.SynchronizeAsync();
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Erro na sincroniza��o: {ex.Message}");
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

        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            // Pode ser usado para atualizar o status de sincroniza��o ou outras informa��es
            // quando o painel de navega��o � aberto
        }
    }
}