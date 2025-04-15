using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using PDV.Domain.Interfaces;
using PDV.Domain.Interfaces.PDV.Domain.Interfaces;
using PDV.Shared.Enum;
using PDV.UI.WinUI3.Services;
using PDV.UI.WinUI3.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PDV.UI.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private Dictionary<string, Type> _pages;

        // Servi�os para sincroniza��o
        private readonly ISyncService _syncService;
        private readonly IConnectivityService _connectivityService;
        private readonly ISyncNotificationService _syncNotificationService;
        private bool _isSynchronizing = false;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomHeader);

            // Configurar o servi�o de notifica��o
            NotificationService.Instance.Initialize(GlobalNotification);

            // Obter servi�os de sincroniza��o
            var app = Microsoft.UI.Xaml.Application.Current as App;
            if (app?.Services != null)
            {
                _syncService = app.Services.GetService<ISyncService>();
                _connectivityService = app.Services.GetService<IConnectivityService>();
                _syncNotificationService = app.Services.GetService<ISyncNotificationService>();

                // Registrar para eventos de sincroniza��o
                if (_syncNotificationService != null)
                {
                    _syncNotificationService.SyncStatusChanged += SyncNotificationService_SyncStatusChanged;
                }
            }

            InitializePages();
            SetupEvents();
            UpdateDateDisplay();
        }

        private void InitializePages()
        {
            _pages = new Dictionary<string, Type>
            {
                { "login", typeof(LoginPage) },
                { "home", typeof(HomePage) },
                { "employees", typeof(EmployeesPage) },
                { "products", typeof(ProductsPage) },
                { "reports", typeof(ReportsPage) },
                { "pos", typeof(POSPage) }
            };
        }

        private void SetupEvents()
        {
            SessionService.Instance.UserLoggedIn += SessionService_UserLoggedIn;
            SessionService.Instance.UserLoggedOut += SessionService_UserLoggedOut;
        }

        private void SessionService_UserLoggedIn(object sender, EventArgs e)
        {
            CustomHeader.Visibility = Visibility.Visible;
            SetTitleBar(CustomHeader);

            UpdateUserInfo();
        }

        private void SessionService_UserLoggedOut(object sender, EventArgs e)
        {
            ContentFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
            NavView.IsPaneVisible = false;
        }

        private void UpdateUserInfo()
        {
            var user = SessionService.Instance.CurrentUser;
            if (user != null)
            {
                UserNameTextBlock.Text = user.Name;
                UserInitialsControl.Initials = GetInitials(user.Name);

                NavView.IsPaneVisible = true;

                UpdateNavViewBasedOnPermissions();
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();
            else
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private void UpdateNavViewBasedOnPermissions()
        {
            // Hide/show navigation items based on user permissions
            // This would be implemented based on the permissions system
        }

        private void UpdateDateDisplay()
        {
            // Implementa��o para atualizar a data/hora atual
        }

        public void NavigateToHomePage()
        {
            NavView.IsPaneVisible = true;

            ContentFrame.Navigate(typeof(HomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            foreach (NavigationViewItem item in NavView.MenuItems)
            {
                if (item.Tag?.ToString() == "home")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }

            UpdateUserInfo();
        }

        public void NavigateToLoginPage()
        {
            NavView.IsPaneVisible = false;

            CustomHeader.Visibility = Visibility.Collapsed;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);

            ContentFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item && item.Tag != null)
            {
                string tag = item.Tag.ToString();

                if (tag == "sync")
                {
                    SyncButton_Tapped(null, null);
                    return;
                }

                if (_pages.TryGetValue(tag, out Type pageType))
                {
                    ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            SessionService.Instance.Logout();

            NotificationService.Instance.ShowInformation("Logout realizado com sucesso.");

            NavigateToLoginPage();
        }

        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            // Qualquer l�gica necess�ria quando o painel de navega��o � aberto
        }

        private async void SyncButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Verificar se j� est� sincronizando
            if (_isSynchronizing)
            {
                NotificationService.Instance.ShowInformation("A sincroniza��o j� est� em andamento.");
                return;
            }

            // Verificar se o servi�o est� dispon�vel
            if (_syncService == null)
            {
                NotificationService.Instance.ShowError("Servi�o de sincroniza��o n�o dispon�vel.");
                return;
            }

            try
            {
                _isSynchronizing = true;
                NotificationService.Instance.ShowInformation("Iniciando sincroniza��o...");

                // Verificar conectividade
                if (_connectivityService != null)
                {
                    await _connectivityService.CheckAndUpdateConnectivityAsync();
                    if (!_connectivityService.IsOnline())
                    {
                        NotificationService.Instance.ShowWarning("N�o � poss�vel sincronizar, dispositivo est� offline.");
                        _isSynchronizing = false;
                        return;
                    }
                }

                // Executar sincroniza��o
                await _syncService.SynchronizeAsync();
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError($"Erro na sincroniza��o: {ex.Message}");
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private void SyncNotificationService_SyncStatusChanged(object sender, SyncStatusEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                switch (e.Status)
                {
                    case SyncStatus.InProgress:
                        NotificationService.Instance.ShowInformation(e.Message);
                        break;
                    case SyncStatus.Completed:
                        NotificationService.Instance.ShowSuccess(e.Message);
                        break;
                    case SyncStatus.Failed:
                        NotificationService.Instance.ShowError(e.Message);
                        break;
                }
            });
        }
    }
}